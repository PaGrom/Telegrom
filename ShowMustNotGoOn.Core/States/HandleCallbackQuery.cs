using System;
using System.Threading;
using System.Threading.Tasks;
using ShowMustNotGoOn.Core.Model;
using Telegrom.Core;
using Telegrom.Core.TelegramModel;
using Telegrom.StateMachine;
using Telegrom.StateMachine.Attributes;

namespace ShowMustNotGoOn.Core.States
{
    public sealed class HandleCallbackQuery : StateBase
    {
        private readonly IStateContext _stateContext;
        private readonly ISessionAttributesService _sessionAttributesService;

        [Output]
        public Callback Callback { get; set; }

        [Output]
        public BotMessage BotMessage { get; set; }

        [Output]
        public TvShowInfo CurrentTvShowInfo { get; set; }

        public HandleCallbackQuery(IStateContext stateContext,
            ISessionAttributesService sessionAttributesService)
        {
            _stateContext = stateContext;
            _sessionAttributesService = sessionAttributesService;
        }

        public override async Task OnEnter(CancellationToken cancellationToken)
        {
            var callbackQuery = _stateContext.UpdateContext.Update as CallbackQuery;

            var callbackId = Guid.Parse(callbackQuery.Data);

            Callback = await _sessionAttributesService.GetSessionAttributeAsync<Callback>(callbackId, cancellationToken);

            BotMessage = await _sessionAttributesService.GetSessionAttributeAsync<BotMessage>(Callback.BotMessageId, cancellationToken);

            CurrentTvShowInfo = new TvShowInfo
            {
                MyShowsId = BotMessage.TvShowInfo.MyShowsId

            };
        }
    }
}
