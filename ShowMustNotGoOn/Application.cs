using System.Linq;
using System.Threading.Tasks;
using Serilog;
using ShowMustNotGoOn.Core;
using ShowMustNotGoOn.Core.MessageBus;
using ShowMustNotGoOn.Messages;
using ShowMustNotGoOn.Messages.Commands;
using ShowMustNotGoOn.Messages.Event;

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

            _messageBus.RegisterHandler<AddTvShowToDbCommand>(async r =>
            {
                var tvShow = await _dbRepository.AddNewTvShowAsync(r.TvShow);
                _logger.Information($"TvShow {tvShow.Title} added to db");
                await _messageBus.Enqueue(new TvShowAddedToDbEvent(tvShow));
            });

            _messageBus.RegisterHandler<TvShowFoundEvent>(async e =>
                await _messageBus.Enqueue(new AddTvShowToDbCommand(e.TvShow)));

            _messageBus.RegisterHandler<SearchTvShowByNameCommand>(async r =>
            {
                _logger.Information($"Searching TV Show by name {r.Name} at position {r.Position}");
                var tvShows = await _tvShowsRepository.SearchTvShowsAsync(r.Name);
                var shows = tvShows.ToArray();
                _logger.Information($"Found {shows.Length} by name {r.Name}");
                var show = shows[r.Position];
                await _messageBus.Enqueue(new TvShowFoundEvent(show));
            });

            await _messageBus.Enqueue(new SearchTvShowByNameCommand("Dark"));

            await Task.Delay(1000000);

        }
    }
}
