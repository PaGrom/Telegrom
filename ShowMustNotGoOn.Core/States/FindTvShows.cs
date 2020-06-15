using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShowMustNotGoOn.Core.Model;
using Telegrom.Core.TelegramModel;
using Telegrom.StateMachine;
using Telegrom.StateMachine.Attributes;

namespace ShowMustNotGoOn.Core.States
{
    public sealed class FindTvShows : StateBase
    {
        private readonly IStateContext _stateContext;
        private readonly ITvShowsService _tvShowsService;

        [Output]
        public List<TvShowInfo> TvShowsInfos { get; set; }

        public FindTvShows(IStateContext stateContext, ITvShowsService tvShowsService)
        {
            _stateContext = stateContext;
            _tvShowsService = tvShowsService;
        }

        public override async Task OnEnter(CancellationToken cancellationToken)
        {
            TvShowsInfos = (await _tvShowsService.SearchTvShowsAsync(((Message)_stateContext.UpdateContext.Update).Text.Trim(), cancellationToken)).ToList();
        }
    }
}
