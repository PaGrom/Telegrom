using System.Collections.Generic;
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
    internal sealed class HandlePrevCallbackQuery : StateBase
    {
        private readonly IGlobalAttributesService _globalAttributesService;
        private readonly ITvShowsService _tvShowsService;

        [Input]
        [Output]
        public BotMessage BotMessage { get; set; }

        [Input]
        [Output]
        public TvShow CurrentTvShow { get; set; }

        [Output]
        public List<TvShow> TvShows { get; set; }

        public HandlePrevCallbackQuery(IGlobalAttributesService globalAttributesService,
            ITvShowsService tvShowsService)
        {
            _globalAttributesService = globalAttributesService;
            _tvShowsService = tvShowsService;
        }

        public override async Task OnEnter(CancellationToken cancellationToken)
        {
            var messageText = await _globalAttributesService.GetGlobalAttributeAsync<MessageText>(BotMessage.MessageTextId, cancellationToken);

            TvShows = (await _tvShowsService.SearchTvShowsAsync(messageText.Text, cancellationToken)).ToList();

            var currentIndex = TvShows.FindIndex(s => s.Id == CurrentTvShow.Id);

            var nextIndex = --currentIndex;

            if (nextIndex < 0)
            {
                nextIndex = 0;
            }

            CurrentTvShow = TvShows[nextIndex];

            BotMessage.MyShowsId = CurrentTvShow.Id;
        }
    }
}
