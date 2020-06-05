using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShowMustNotGoOn.Core;
using ShowMustNotGoOn.DatabaseContext.Model;
using ShowMustNotGoOn.StateMachine;
using ShowMustNotGoOn.StateMachine.Attributes;

namespace ShowMustNotGoOn.States
{
    internal sealed class HandlePrevCallbackQuery : StateBase
    {
        private readonly DatabaseContext.DatabaseContext _databaseContext;
        private readonly ITvShowsService _tvShowsService;

        [Input]
        [Output]
        public BotMessage BotMessage { get; set; }

        [Input]
        [Output]
        public TvShow CurrentTvShow { get; set; }

        public HandlePrevCallbackQuery(DatabaseContext.DatabaseContext databaseContext,
            ITvShowsService tvShowsService)
        {
            _databaseContext = databaseContext;
            _tvShowsService = tvShowsService;
        }

        public override async Task OnEnter(CancellationToken cancellationToken)
        {
            var messageText = await _databaseContext.MessageTexts
                .FindAsync(new object[] { BotMessage.MessageTextId }, cancellationToken);

            var tvShows = (await _tvShowsService.SearchTvShowsAsync(messageText.Text, cancellationToken)).ToList();

            var currentIndex = tvShows.FindIndex(s => s.Id == CurrentTvShow.Id);

            var nextIndex = --currentIndex;

            if (nextIndex < 0)
            {
                nextIndex = 0;
            }

            CurrentTvShow = tvShows[nextIndex];

            BotMessage.MyShowsId = CurrentTvShow.Id;
        }
    }
}
