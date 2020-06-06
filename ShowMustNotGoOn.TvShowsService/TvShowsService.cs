using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ShowMustNotGoOn.Core;
using ShowMustNotGoOn.Core.Model;
using Telegrom.Core;

namespace ShowMustNotGoOn.TvShowsService
{
    public class TvShowsService : ITvShowsService
    {
        private readonly IMyShowsService _myShowsService;
        private readonly IGlobalAttributesService _globalAttributesService;
        private readonly ILogger<TvShowsService> _logger;

        public TvShowsService(IMyShowsService myShowsService,
            IGlobalAttributesService globalAttributesService,
            ILogger<TvShowsService> logger)
        {
            _myShowsService = myShowsService;
            _globalAttributesService = globalAttributesService;
            _logger = logger;
        }

        public async Task<Guid> AddNewTvShowAsync(TvShow tvShow, CancellationToken cancellationToken)
        {
            var guid = await _globalAttributesService.GetAttributeIdByValueAsync(tvShow, cancellationToken);

            if (guid != null)
            {
                _logger.LogInformation($"TV Show '{tvShow.Title}' (Id: {tvShow.Id}) already exists in db");
            }
            else
            {
                _logger.LogInformation($"Adding TV Show '{tvShow.Title}' (Id: {tvShow.Id}) to db");
                guid = Guid.NewGuid();
                await _globalAttributesService.CreateOrUpdateGlobalAttributeAsync(guid.Value, tvShow, cancellationToken);
            }

            return guid.Value;
        }

        public Task<IEnumerable<TvShow>> SearchTvShowsAsync(string name, CancellationToken cancellationToken)
        {
            return _myShowsService.SearchTvShowsAsync(name, cancellationToken);
        }

        public Task<TvShow> GetTvShowFromMyShowsAsync(int myShowsId, CancellationToken cancellationToken)
        {
            return _myShowsService.GetTvShowAsync(myShowsId, cancellationToken);
        }
    }
}
