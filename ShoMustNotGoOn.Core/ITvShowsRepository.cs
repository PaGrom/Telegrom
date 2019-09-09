using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShowMustNotGoOn.Core
{
    public interface ITvShowsRepository
    {
        Task<IEnumerable<TvShow>> SearchTvShowsAsync(string name);
    }
}
