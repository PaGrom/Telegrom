using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ShowMustNotGoOn.Core.Model;

namespace ShowMustNotGoOn.Core
{
    public interface ITvShowsService
    {
        Task<Guid> AddNewTvShowAsync(TvShow tvShow, CancellationToken cancellationToken);
        Task<IEnumerable<TvShow>> SearchTvShowsAsync(string name, CancellationToken cancellationToken);
        Task<TvShow> GetTvShowFromMyShowsAsync(int myShowsId, CancellationToken cancellationToken);
    }
}
