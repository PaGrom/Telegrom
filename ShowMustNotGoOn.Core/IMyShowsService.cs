using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ShowMustNotGoOn.Core.Model;

namespace ShowMustNotGoOn.Core
{
    public interface IMyShowsService
    {
        Task<IEnumerable<TvShowInfo>> SearchTvShowsAsync(string name, CancellationToken cancellationToken);
        Task<TvShowDescription> GetTvShowAsync(int tvShowId, CancellationToken cancellationToken);
    }
}
