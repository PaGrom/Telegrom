using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ShowMustNotGoOn.Core;
using ShowMustNotGoOn.Core.TelegramModel;
using ShowMustNotGoOn.DatabaseContext.Model;
using ShowMustNotGoOn.StateMachine;
using ShowMustNotGoOn.StateMachine.Attributes;

namespace ShowMustNotGoOn.States
{
    internal sealed class GenerateSubscriptionsButtons : StateBase
    {
        private readonly IStateContext _stateContext;
        private readonly DatabaseContext.DatabaseContext _databaseContext;
        private readonly ITvShowsService _tvShowsService;

        [Input]
        public TvShow CurrentTvShow { get; set; }

        [Input]
        public BotMessage BotMessage { get; set; }

        [Input]
        [Output]
        public InlineKeyboardMarkup InlineKeyboardMarkup { get; set; }

        public GenerateSubscriptionsButtons(IStateContext stateContext,
            DatabaseContext.DatabaseContext databaseContext,
            ITvShowsService tvShowsService)
        {
            _stateContext = stateContext;
            _databaseContext = databaseContext;
            _tvShowsService = tvShowsService;
        }

        public override async Task OnEnter(CancellationToken cancellationToken)
        {
            var buttons = new List<InlineKeyboardButton>();

            var subscription = await _tvShowsService
                .GetUserSubscriptionToTvShowAsync(_stateContext.UpdateContext.SessionContext.User, CurrentTvShow, SubscriptionType.EndOfShow, cancellationToken);

            if (subscription != null)
            {
                var callback = await CreateCallbackAsync(BotMessage.Id, CallbackType.UnsubscribeToEndOfShow, cancellationToken);
                buttons.Add(InlineKeyboardButton.WithCallbackData("Unsubscribe from end of show", callback.Id.ToString()));
            }
            else
            {
                var callback = await CreateCallbackAsync(BotMessage.Id, CallbackType.SubscribeToEndOfShow, cancellationToken);
                buttons.Add(InlineKeyboardButton.WithCallbackData("Subscribe to end of show", callback.Id.ToString()));
            }

            InlineKeyboardMarkup.AddRow(buttons);

            async Task<Callback> CreateCallbackAsync(int botMessageId, CallbackType callbackType, CancellationToken cancellationToken)
            {
                var callback = (await _databaseContext.Callbacks
                    .AddAsync(new Callback
                    {
                        BotMessageId = botMessageId,
                        CallbackType = callbackType
                    }, cancellationToken)).Entity;
                await _databaseContext.SaveChangesAsync(cancellationToken);

                return callback;
            }
        }
    }
}
