using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShowMustNotGoOn.Core;
using ShowMustNotGoOn.Core.TelegramModel;
using ShowMustNotGoOn.DatabaseContext.Model;
using ShowMustNotGoOn.StateMachine;
using ShowMustNotGoOn.StateMachine.Attributes;

namespace ShowMustNotGoOn.States
{
    internal sealed class HandleCallbackQuery : StateBase
    {
        private readonly IStateContext _stateContext;
        private readonly DatabaseContext.DatabaseContext _databaseContext;
        private readonly ITvShowsService _tvShowsService;

        [Output]
        public Callback Callback { get; set; }

        [Output]
        public BotMessage BotMessage { get; set; }

        [Output]
        public TvShow CurrentTvShow { get; set; }

        public HandleCallbackQuery(IStateContext stateContext,
            DatabaseContext.DatabaseContext databaseContext,
            ITvShowsService tvShowsService)
        {
            _stateContext = stateContext;
            _databaseContext = databaseContext;
            _tvShowsService = tvShowsService;
        }

        public override async Task OnEnter(CancellationToken cancellationToken)
        {
            var callbackQuery = _stateContext.UpdateContext.Update as CallbackQuery;

            var callbackId = Guid.Parse(callbackQuery.Data);

            Callback = await _databaseContext.Callbacks.FindAsync(new object[] { callbackId }, cancellationToken);

            BotMessage = await _databaseContext.BotMessages
                .FindAsync(new object[] { Callback.BotMessageId }, cancellationToken);

            _databaseContext.Entry(BotMessage).State = EntityState.Detached;

            CurrentTvShow = await _tvShowsService.GetTvShowByMyShowsIdAsync(BotMessage.MyShowsId, cancellationToken)
                            ?? await _tvShowsService.GetTvShowFromMyShowsAsync(BotMessage.MyShowsId, cancellationToken);
        }
    }
}
