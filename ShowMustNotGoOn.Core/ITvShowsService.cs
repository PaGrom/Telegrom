using System.Collections.Generic;
using System.Threading.Tasks;
using ShowMustNotGoOn.Core.Model;

namespace ShowMustNotGoOn.Core
{
    public interface ITvShowsService
    {
        Task<TvShow> AddNewTvShowAsync(TvShow tvShow);
        Task<IEnumerable<TvShow>> SearchTvShowsAsync(string name);
    }
}