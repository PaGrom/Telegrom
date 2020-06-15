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
        const string NotFoundImage = "https://images-na.ssl-images-amazon.com/images/I/312yeogBelL._SX466_.jpg";

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

        public async Task<Guid> AddNewTvShowAsync(TvShowDescription tvShowDescription, CancellationToken cancellationToken)
        {
            var guid = await _globalAttributesService.GetAttributeIdByValueAsync(tvShowDescription, cancellationToken);

            if (guid != null)
            {
                _logger.LogInformation($"TV Show '{tvShowDescription.Title}' (Id: {tvShowDescription.Id}) already exists in db");
            }
            else
            {
                _logger.LogInformation($"Adding TV Show '{tvShowDescription.Title}' (Id: {tvShowDescription.Id}) to db");
                guid = Guid.NewGuid();
                await _globalAttributesService.CreateOrUpdateGlobalAttributeAsync(guid.Value, tvShowDescription, cancellationToken);
            }

            return guid.Value;
        }

        public Task<IEnumerable<TvShowInfo>> SearchTvShowsAsync(string name, CancellationToken cancellationToken)
        {
            return _myShowsService.SearchTvShowsAsync(name, cancellationToken);
        }

        public async Task<TvShowDescription> GetTvShowDescriptionAsync(int myShowsId, CancellationToken cancellationToken)
        {
            var tvShowDescription = await _myShowsService.GetTvShowAsync(myShowsId, cancellationToken);

            if (string.IsNullOrEmpty(tvShowDescription.Image))
            {
                tvShowDescription.Image = NotFoundImage;
            }

            return tvShowDescription;
        }
    }
}
