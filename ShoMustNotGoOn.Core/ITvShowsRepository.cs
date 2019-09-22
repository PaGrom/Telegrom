using System.Collections.Generic;
using System.Threading.Tasks;
using ShowMustNotGoOn.Core.Model;

namespace ShowMustNotGoOn.Core
{
    public interface ITvShowsRepository
    {
        Task<IEnumerable<TvShow>> SearchTvShowsAsync(string name);
    }
}
