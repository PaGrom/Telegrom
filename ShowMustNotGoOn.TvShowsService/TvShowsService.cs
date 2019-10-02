using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Serilog;
using ShowMustNotGoOn.Core;
using ShowMustNotGoOn.Core.Model;

namespace ShowMustNotGoOn.TvShowsService
{
    public class TvShowsService : ITvShowsService
    {
        private readonly IMyShowsService _myShowsService;
        private readonly DatabaseContext.DatabaseContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public TvShowsService(IMyShowsService myShowsService,
            DatabaseContext.DatabaseContext dbContext,
            IMapper mapper,
            ILogger logger)
        {
            _myShowsService = myShowsService;
            _dbContext = dbContext;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<TvShow> AddNewTvShowAsync(TvShow tvShow)
        {
            TvShow show;
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            var existingShow = await _dbContext.TvShows.SingleOrDefaultAsync(s => s.MyShowsId == tvShow.MyShowsId);
            if (existingShow != null)
            {
                _logger.Information($"TV Show '{existingShow.Title}' (Id: {existingShow.MyShowsId}) already exists in db");
                show = existingShow;
            }
            else
            {
                _logger.Information($"Adding TV Show '{tvShow.Title}' (Id: {tvShow.MyShowsId}) to db");
                show = _dbContext.TvShows.Add(tvShow).Entity;
                await _dbContext.SaveChangesAsync();
            }

            transaction.Commit();

            return show;
        }

        public async Task<IEnumerable<TvShow>> SearchTvShowsAsync(string name)
        {
            return await _myShowsService.SearchTvShowsAsync(name);
        }

        public async Task<TvShow> GetTvShowAsync(int tvShowId)
        {
            return await _myShowsService.GetTvShowAsync(tvShowId);
        }
    }
}
