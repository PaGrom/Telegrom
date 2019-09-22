using System.Threading.Tasks;
using ShowMustNotGoOn.Core.Model;

namespace ShowMustNotGoOn.Core
{
    public interface IDatabaseRepository
    {
        Task<TvShow> AddNewTvShowAsync(TvShow tvShow);
        Task<User> AddOrUpdateUserAsync(User user);
    }
}
