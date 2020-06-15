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
    public sealed class GenerateTvShowsBotMessage : StateBase
    {
        private readonly IStateContext _stateContext;
        private readonly IGlobalAttributesService _globalAttributesService;
        private readonly ISessionAttributesService _sessionAttributesService;

        [Input]
        public List<TvShowInfo> TvShowsInfos { get; set; }

        [Output]
        public TvShowInfo CurrentTvShowInfo { get; set; }

        [Output]
        public BotMessage BotMessage { get; set; }

        public GenerateTvShowsBotMessage(IStateContext stateContext,
            IGlobalAttributesService globalAttributesService,
            ISessionAttributesService sessionAttributesService)
        {
            _stateContext = stateContext;
            _globalAttributesService = globalAttributesService;
            _sessionAttributesService = sessionAttributesService;
        }

        public override async Task OnEnter(CancellationToken cancellationToken)
        {
            var messageTextString = ((Message)_stateContext.UpdateContext.Update).Text.Trim();

            var messageText = new MessageText
            {
                Text = messageTextString
            };

            var messageTextId = await _globalAttributesService.GetAttributeIdByValueAsync(messageText, cancellationToken);

            if (messageTextId == null)
            {
                messageTextId = Guid.NewGuid();

                await _globalAttributesService.CreateOrUpdateGlobalAttributeAsync(messageTextId.Value, messageText, cancellationToken);
            }

            CurrentTvShowInfo = TvShowsInfos.First();

            BotMessage = new BotMessage
            {
                Id = Guid.NewGuid(),
                BotCommandType = BotCommandType.NotCommand,
                MessageTextId = messageTextId.Value,
                TvShowInfo = CurrentTvShowInfo
            };

            await _sessionAttributesService.SaveOrUpdateSessionAttributeAsync(BotMessage.Id, BotMessage, cancellationToken);
        }
    }
}
