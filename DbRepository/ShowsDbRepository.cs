using System.Threading.Tasks;
using AutoMapper;
using DbRepository.Entities;
using ShowMustNotGoOn.Core;

namespace DbRepository
{
    public class ShowsDbRepository : IShowsDbRepository
    {
        private readonly ShowsDbContext _dbContext;
        private readonly IMapper _mapper;

        public ShowsDbRepository(ShowsDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<TvShow> AddNewTvShowAsync(TvShow tvShow)
        {
            var show = _dbContext.TvShows.Add(_mapper.Map<TvShows>(tvShow)).Entity;
            await _dbContext.SaveChangesAsync();
            return _mapper.Map<TvShow>(show);
        }
    }
}
