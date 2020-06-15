using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShowMustNotGoOn.Core.Model;
using Telegrom.Core;
using Telegrom.StateMachine;
using Telegrom.StateMachine.Attributes;

namespace ShowMustNotGoOn.Core.States
{
    public sealed class HandlePrevCallbackQuery : StateBase
    {
        private readonly IGlobalAttributesService _globalAttributesService;
        private readonly ITvShowsService _tvShowsService;

        [Input]
        [Output]
        public BotMessage BotMessage { get; set; }

        [Input]
        [Output]
        public TvShowInfo CurrentTvShowInfo { get; set; }

        [Output]
        public List<TvShowInfo> TvShowsInfos { get; set; }

        public HandlePrevCallbackQuery(IGlobalAttributesService globalAttributesService,
            ITvShowsService tvShowsService)
        {
            _globalAttributesService = globalAttributesService;
            _tvShowsService = tvShowsService;
        }

        public override async Task OnEnter(CancellationToken cancellationToken)
        {
            var messageText = await _globalAttributesService.GetGlobalAttributeAsync<MessageText>(BotMessage.MessageTextId, cancellationToken);

            TvShowsInfos = (await _tvShowsService.SearchTvShowsAsync(messageText.Text, cancellationToken)).ToList();

            var currentIndex = TvShowsInfos.FindIndex(s => s.MyShowsId == CurrentTvShowInfo.MyShowsId);

            var nextIndex = --currentIndex;

            if (nextIndex < 0)
            {
                nextIndex = 0;
            }

            CurrentTvShowInfo = TvShowsInfos[nextIndex];

            BotMessage.TvShowInfo = CurrentTvShowInfo;
        }
    }
}
