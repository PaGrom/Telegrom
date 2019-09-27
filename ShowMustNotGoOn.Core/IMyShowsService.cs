using System.Collections.Generic;
using System.Threading.Tasks;
using ShowMustNotGoOn.Core.Model;

namespace ShowMustNotGoOn.Core
{
    public interface IMyShowsService
    {
        Task<IEnumerable<TvShow>> SearchTvShowsAsync(string name);
    }
}
