using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShowMustNotGoOn.Core;
using ShowMustNotGoOn.Core.TelegramModel;
using ShowMustNotGoOn.DatabaseContext.Model;
using ShowMustNotGoOn.StateMachine;
using ShowMustNotGoOn.StateMachine.Attributes;

namespace ShowMustNotGoOn.States
{
    internal sealed class FindTvShows : StateBase
    {
        private readonly IStateContext _stateContext;
        private readonly ITvShowsService _tvShowsService;

        [Output]
        public List<TvShow> TvShows { get; set; }

        public FindTvShows(IStateContext stateContext, ITvShowsService tvShowsService)
        {
            _stateContext = stateContext;
            _tvShowsService = tvShowsService;
        }

        public override async Task OnEnter(CancellationToken cancellationToken)
        {
            TvShows = (await _tvShowsService.SearchTvShowsAsync(((Message)_stateContext.UpdateContext.Update).Text.Trim(), cancellationToken)).ToList();
        }
    }
}
