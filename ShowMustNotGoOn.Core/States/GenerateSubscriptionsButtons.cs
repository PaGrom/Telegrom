using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShowMustNotGoOn.Core.Model;
using Telegrom.Core;
using Telegrom.Core.TelegramModel;
using Telegrom.StateMachine;
using Telegrom.StateMachine.Attributes;

namespace ShowMustNotGoOn.Core.States
{
    public sealed class GenerateSubscriptionsButtons : StateBase
    {
        private readonly ISessionAttributesService _sessionAttributesService;

        [Input]
        public TvShowInfo CurrentTvShowInfo { get; set; }

        [Input]
        public BotMessage BotMessage { get; set; }

        [Input]
        [Output]
        public InlineKeyboardMarkup InlineKeyboardMarkup { get; set; }

        public GenerateSubscriptionsButtons(ISessionAttributesService sessionAttributesService)
        {
            _sessionAttributesService = sessionAttributesService;
        }

        public override async Task OnEnter(CancellationToken cancellationToken)
        {
            var buttons = new List<InlineKeyboardButton>();

            var subscription = (await _sessionAttributesService
                .GetAllByTypeAsync<Subscription>(cancellationToken))
                .SingleOrDefault(s => s.TvShowId == CurrentTvShowInfo.MyShowsId
                                      && s.SubscriptionType == SubscriptionType.EndOfShow);

            if (subscription != null)
            {
                var callback = await CreateCallbackAsync(BotMessage.Id, CallbackType.UnsubscribeToEndOfShow);
                buttons.Add(InlineKeyboardButton.WithCallbackData("Unsubscribe from end of show", callback.Id.ToString()));
            }
            else
            {
                var callback = await CreateCallbackAsync(BotMessage.Id, CallbackType.SubscribeToEndOfShow);
                buttons.Add(InlineKeyboardButton.WithCallbackData("Subscribe to end of show", callback.Id.ToString()));
            }

            InlineKeyboardMarkup.AddRow(buttons);

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
