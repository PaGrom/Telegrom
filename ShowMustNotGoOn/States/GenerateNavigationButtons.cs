using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ShowMustNotGoOn.Core.TelegramModel;
using ShowMustNotGoOn.DatabaseContext.Model;
using ShowMustNotGoOn.StateMachine;
using ShowMustNotGoOn.StateMachine.Attributes;

namespace ShowMustNotGoOn.States
{
    internal sealed class GenerateNavigationButtons : StateBase
    {
        private readonly IStateContext _stateContext;
        private readonly DatabaseContext.DatabaseContext _databaseContext;

        [Input]
        public List<TvShow> TvShows { get; set; }

        [Input]
        public TvShow CurrentTvShow { get; set; }

        [Input]
        public BotMessage BotMessage { get; set; }

        [Input]
        [Output]
        public InlineKeyboardMarkup InlineKeyboardMarkup { get; set; }

        public GenerateNavigationButtons(IStateContext stateContext, DatabaseContext.DatabaseContext databaseContext)
        {
            _stateContext = stateContext;
            _databaseContext = databaseContext;
        }

        public override async Task OnEnter(CancellationToken cancellationToken)
        {
            var buttons = new List<InlineKeyboardButton>();

            var tvShowIndex = TvShows.FindIndex(s => s.Id == CurrentTvShow.Id);
            
            if (tvShowIndex > 0)
            {
                var callback = await CreateCallbackAsync(BotMessage.Id, CallbackType.Prev);
                buttons.Add(InlineKeyboardButton.WithCallbackData("Prev", callback.Id.ToString()));
            }

            if (tvShowIndex < TvShows.Count - 1)
            {
                var callback = await CreateCallbackAsync(BotMessage.Id, CallbackType.Next);
                buttons.Add(InlineKeyboardButton.WithCallbackData("Next", callback.Id.ToString()));
            }

            InlineKeyboardMarkup.AddRow(buttons);

            // TODO: Move to UnitOfWork
            async Task<Callback> CreateCallbackAsync(int botMessageId, CallbackType callbackType)
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
