using System.Linq;
using System.Threading.Tasks;
using Serilog;
using ShowMustNotGoOn.Core;
using ShowMustNotGoOn.Core.MessageBus;
using ShowMustNotGoOn.Messages;

namespace ShowMustNotGoOn
{
    public class Application
    {
        private readonly ITvShowsRepository _tvShowsRepository;
        private readonly IMessageBus _messageBus;
        private readonly IShowsDbRepository _dbRepository;
        private readonly ILogger _logger;

        public Application(ITvShowsRepository tvShowsRepository,
            IMessageBus messageBus,
            IShowsDbRepository dbRepository,
            ILogger logger)
        {
            _tvShowsRepository = tvShowsRepository;
            _messageBus = messageBus;
            _dbRepository = dbRepository;
            _logger = logger;

            Task.Factory.StartNew(async () => { await RunAsync(); },
                TaskCreationOptions.LongRunning);
        }

        public async Task RunAsync()
        {
            _logger.Information("Application start");

            _messageBus.RegisterHandler<SaveTvShowToDb>(async r =>
            {
                var tvShow = await _dbRepository.AddNewTvShowAsync(r.TvShow);
            });

            _messageBus.RegisterHandler<RequestTvShow>(async r =>
            {
                var tvShows = await _tvShowsRepository.SearchTvShowsAsync(r.Name);
                var shows = tvShows.ToList();
                _logger.Information($"Found {shows.Count} by name {r.Name}");
                await _messageBus.Enqueue(new SaveTvShowToDb
                {
                    TvShow = shows.First()
                });
            });

            await _messageBus.Enqueue(new RequestTvShow
            {
                Name = "Dark"
            });

            await Task.Delay(1000000);

        }
    }
}
