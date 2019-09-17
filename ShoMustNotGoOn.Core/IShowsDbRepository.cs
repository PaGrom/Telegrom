using System.Threading.Tasks;

namespace ShowMustNotGoOn.Core
{
    public interface IShowsDbRepository
    {
        Task<TvShow> AddNewTvShowAsync(TvShow tvShow);
    }
}
