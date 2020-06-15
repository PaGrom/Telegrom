using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ShowMustNotGoOn.Core.Model;
using Telegrom.Core;
using Telegrom.Core.TelegramModel;
using Telegrom.StateMachine;
using Telegrom.StateMachine.Attributes;

namespace ShowMustNotGoOn.Core.States
{
    public sealed class GenerateNavigationButtons : StateBase
    {
        private readonly ISessionAttributesService _sessionAttributesService;

        [Input]
        public List<TvShowInfo> TvShowsInfos { get; set; }

        [Input]
        public TvShowInfo CurrentTvShowInfo { get; set; }

        [Input]
        public BotMessage BotMessage { get; set; }

        [Input]
        [Output]
        public InlineKeyboardMarkup InlineKeyboardMarkup { get; set; }

        public GenerateNavigationButtons(ISessionAttributesService sessionAttributesService)
        {
            _sessionAttributesService = sessionAttributesService;
        }

        public override async Task OnEnter(CancellationToken cancellationToken)
        {
            var buttons = new List<InlineKeyboardButton>();

            var tvShowIndex = TvShowsInfos.FindIndex(s => s.MyShowsId == CurrentTvShowInfo.MyShowsId);
            
            if (tvShowIndex > 0)
            {
                var callback = await CreateCallbackAsync(BotMessage.Id, CallbackType.Prev);
                buttons.Add(InlineKeyboardButton.WithCallbackData("Prev", callback.Id.ToString()));
            }

            if (tvShowIndex < TvShowsInfos.Count - 1)
            {
                var callback = await CreateCallbackAsync(BotMessage.Id, CallbackType.Next);
                buttons.Add(InlineKeyboardButton.WithCallbackData("Next", callback.Id.ToString()));
            }

            InlineKeyboardMarkup.AddRow(buttons);

            // TODO: Move to UnitOfWork
            async Task<Callback> CreateCallbackAsync(Guid botMessageId, CallbackType callbackType)
            {
                var callback = new Callback
                {
                    Id = Guid.NewGuid(),
                    BotMessageId = botMessageId,
                    CallbackType = callbackType
                };

                await _sessionAttributesService.SaveOrUpdateSessionAttributeAsync(callback.Id, callback, cancellationToken);

                return callback;
            }
        }
    }
}
