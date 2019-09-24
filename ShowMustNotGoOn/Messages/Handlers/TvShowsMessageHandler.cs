using System;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using ShowMustNotGoOn.Core;
using ShowMustNotGoOn.Core.MessageBus;
using ShowMustNotGoOn.Messages.Commands;
using ShowMustNotGoOn.Messages.Event;

namespace ShowMustNotGoOn.Messages.Handlers
{
    public sealed class TvShowsMessageHandler : IDisposable
    {
        private readonly ITvShowsService _tvShowsService;
        private readonly IMessageBus _messageBus;
        private readonly ILogger _logger;

        public TvShowsMessageHandler(ITvShowsService tvShowsService,
            IMessageBus messageBus,
            ILogger logger)
        {
            _tvShowsService = tvShowsService;
            _messageBus = messageBus;
            _logger = logger;

            RegisterHandlers();
        }

        public void Dispose()
        {
            UnregisterHandlers();
        }

        private void RegisterHandlers()
        {
            _messageBus.RegisterHandler<AddTvShowToDbCommand>(HandleAddTvShowToDbCommand);
            _messageBus.RegisterHandler<SearchTvShowByNameCommand>(HandleSearchTvShowByNameCommand);
            _messageBus.RegisterHandler<TvShowFoundEvent>(HandleTvShowFoundEvent);
        }

        private void UnregisterHandlers()
        {
            _messageBus.UnregisterHandler<AddTvShowToDbCommand>(HandleAddTvShowToDbCommand);
            _messageBus.UnregisterHandler<SearchTvShowByNameCommand>(HandleSearchTvShowByNameCommand);
            _messageBus.UnregisterHandler<TvShowFoundEvent>(HandleTvShowFoundEvent);
        }

        private async Task HandleAddTvShowToDbCommand(AddTvShowToDbCommand r)
        {
            var tvShow = await _tvShowsService.AddNewTvShowAsync(r.TvShow);
            _logger.Information($"TvShow {tvShow.Title} added to db");
            await _messageBus.Enqueue(new TvShowAddedToDbEvent(tvShow));
        }

        private async Task HandleTvShowFoundEvent(TvShowFoundEvent @event)
        {
            await _messageBus.Enqueue(new AddTvShowToDbCommand(@event.TvShow));
        }

        private async Task HandleSearchTvShowByNameCommand(SearchTvShowByNameCommand command)
        {
            _logger.Information($"Searching TV Show by name {command.Name} at position {command.Position}");
            var tvShows = await _tvShowsService.SearchTvShowsAsync(command.Name);
            var shows = tvShows.ToArray();
            _logger.Information($"Found {shows.Length} by name {command.Name}");
            var show = shows[command.Position];
            await _messageBus.Enqueue(new TvShowFoundEvent(show));
        }
    }
}
