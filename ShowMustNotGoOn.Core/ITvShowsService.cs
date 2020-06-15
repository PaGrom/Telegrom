using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ShowMustNotGoOn.Core.Model;

namespace ShowMustNotGoOn.Core
{
    public interface ITvShowsService
    {
        Task<Guid> AddNewTvShowAsync(TvShowDescription tvShowDescription, CancellationToken cancellationToken);
        Task<IEnumerable<TvShowInfo>> SearchTvShowsAsync(string name, CancellationToken cancellationToken);
        Task<TvShowDescription> GetTvShowDescriptionAsync(int myShowsId, CancellationToken cancellationToken);
    }
}
