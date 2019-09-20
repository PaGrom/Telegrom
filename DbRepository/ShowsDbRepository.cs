using System.Threading.Tasks;
using AutoMapper;
using DbRepository.Entities;
using Microsoft.EntityFrameworkCore;
using Serilog;
using ShowMustNotGoOn.Core;

namespace DbRepository
{
    public class ShowsDbRepository : IShowsDbRepository
    {
        private readonly ShowsDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public ShowsDbRepository(ShowsDbContext dbContext, IMapper mapper, ILogger logger)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<TvShow> AddNewTvShowAsync(TvShow tvShow)
        {
            var show = _mapper.Map<TvShows>(tvShow);
            using var transaction = _dbContext.Database.BeginTransaction();
            var existingShow = await _dbContext.TvShows.SingleOrDefaultAsync(s => s.MyShowsId == show.MyShowsId);
            if (existingShow != null)
            {
                _logger.Information($"TV Show '{existingShow.Title}' (Id: {existingShow.MyShowsId}) already exists in db");
                show = existingShow;
            }
            else
            {
                _logger.Information($"Adding TV Show '{existingShow.Title}' (Id: {existingShow.MyShowsId}) to db");
                show = _dbContext.TvShows.Add(show).Entity;
                await _dbContext.SaveChangesAsync();
            }

            transaction.Commit();

            return _mapper.Map<TvShow>(show);
        }
    }
}
