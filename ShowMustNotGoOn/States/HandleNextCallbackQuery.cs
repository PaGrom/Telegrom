using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShowMustNotGoOn.Core;
using ShowMustNotGoOn.Core.Model;
using Telegrom.Core;
using Telegrom.StateMachine;
using Telegrom.StateMachine.Attributes;

namespace ShowMustNotGoOn.States
{
    internal sealed class HandleNextCallbackQuery : StateBase
    {
        private readonly ISessionAttributesService _sessionAttributesService;
        private readonly ITvShowsService _tvShowsService;

        [Input]
        [Output]
        public BotMessage BotMessage { get; set; }

        [Input]
        [Output]
        public TvShow CurrentTvShow { get; set; }

        public HandleNextCallbackQuery(ISessionAttributesService sessionAttributesService,
            ITvShowsService tvShowsService)
        {
            _sessionAttributesService = sessionAttributesService;
            _tvShowsService = tvShowsService;
        }

        public override async Task OnEnter(CancellationToken cancellationToken)
        {
            var messageText = await _sessionAttributesService.GetSessionAttributeAsync<MessageText>(BotMessage.MessageTextId, cancellationToken);

            var tvShows = (await _tvShowsService.SearchTvShowsAsync(messageText.Text, cancellationToken)).ToList();

            var currentIndex = tvShows.FindIndex(s => s.Id == CurrentTvShow.Id);

            var nextIndex = ++currentIndex;

            if (nextIndex >= tvShows.Count)
            {
                nextIndex = 0;
            }

            CurrentTvShow = tvShows[nextIndex];

            BotMessage.MyShowsId = CurrentTvShow.Id;
        }
    }
}
