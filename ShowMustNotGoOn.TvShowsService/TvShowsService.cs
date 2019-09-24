using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Serilog;
using ShowMustNotGoOn.Core;
using ShowMustNotGoOn.Core.Model;
using ShowMustNotGoOn.DatabaseContext.Entities;

namespace ShowMustNotGoOn.TvShowsService
{
    public class TvShowsService : ITvShowsService
    {
        private readonly ITvShowsRepository _tvShowsRepository;
        private readonly ShowsDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public TvShowsService(ITvShowsRepository tvShowsRepository,
            ShowsDbContext dbContext,
            IMapper mapper,
            ILogger logger)
        {
            _tvShowsRepository = tvShowsRepository;
            _dbContext = dbContext;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<TvShow> AddNewTvShowAsync(TvShow tvShow)
        {
            var show = _mapper.Map<TvShows>(tvShow);
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
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

        public async Task<IEnumerable<TvShow>> SearchTvShowsAsync(string name)
        {
            return await _tvShowsRepository.SearchTvShowsAsync(name);
        }
    }
}
