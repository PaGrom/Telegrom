using System;
using System.Linq;
using Serilog;
using ShowMustNotGoOn.Core;
using ShowMustNotGoOn.Core.MessageBus;
using ShowMustNotGoOn.Messages.Commands;
using ShowMustNotGoOn.Messages.Event;

namespace ShowMustNotGoOn.Messages.Handlers
{
    public sealed class MyShowsMessageHandler : IDisposable
    {
        private readonly ITvShowsRepository _tvShowsRepository;
        private readonly IMessageBus _messageBus;
        private readonly ILogger _logger;

        public MyShowsMessageHandler(ITvShowsRepository tvShowsRepository,
            IMessageBus messageBus,
            ILogger logger)
        {
            _tvShowsRepository = tvShowsRepository;
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
            _messageBus.RegisterHandler<SearchTvShowByNameCommand>(HandleSearchTvShowByNameCommand);
            _messageBus.RegisterHandler<TvShowFoundEvent>(HandleTvShowFoundEvent);
        }

        private void UnregisterHandlers()
        {
            _messageBus.UnregisterHandler<SearchTvShowByNameCommand>(HandleSearchTvShowByNameCommand);
            _messageBus.UnregisterHandler<TvShowFoundEvent>(HandleTvShowFoundEvent);
        }

        private async void HandleTvShowFoundEvent(TvShowFoundEvent @event)
        {
            await _messageBus.Enqueue(new AddTvShowToDbCommand(@event.TvShow));
        }

        private async void HandleSearchTvShowByNameCommand(SearchTvShowByNameCommand command)
        {
            _logger.Information($"Searching TV Show by name {command.Name} at position {command.Position}");
            var tvShows = await _tvShowsRepository.SearchTvShowsAsync(command.Name);
            var shows = tvShows.ToArray();
            _logger.Information($"Found {shows.Length} by name {command.Name}");
            var show = shows[command.Position];
            await _messageBus.Enqueue(new TvShowFoundEvent(show));
        }
    }
}
