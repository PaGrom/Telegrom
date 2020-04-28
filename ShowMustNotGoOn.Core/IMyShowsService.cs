using System.Collections.Generic;
using System.Threading.Tasks;
using ShowMustNotGoOn.DatabaseContext.Model;

namespace ShowMustNotGoOn.Core
{
    public interface IMyShowsService
    {
        Task<IEnumerable<TvShow>> SearchTvShowsAsync(string name);
        Task<TvShow> GetTvShowAsync(int tvShowId);
    }
}
