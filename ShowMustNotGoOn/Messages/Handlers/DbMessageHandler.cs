using System;
using Serilog;
using ShowMustNotGoOn.Core;
using ShowMustNotGoOn.Core.MessageBus;
using ShowMustNotGoOn.Messages.Commands;
using ShowMustNotGoOn.Messages.Event;

namespace ShowMustNotGoOn.Messages.Handlers
{
    public sealed class DbMessageHandler : IDisposable
    {
        private readonly IShowsDbRepository _dbRepository;
        private readonly IMessageBus _messageBus;
        private readonly ILogger _logger;

        public DbMessageHandler(IShowsDbRepository dbRepository,
            IMessageBus messageBus,
            ILogger logger)
        {
            _dbRepository = dbRepository;
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
        }

        private void UnregisterHandlers()
        {
            _messageBus.UnregisterHandler<AddTvShowToDbCommand>(HandleAddTvShowToDbCommand);
        }

        private async void HandleAddTvShowToDbCommand(AddTvShowToDbCommand r)
        {
            var tvShow = await _dbRepository.AddNewTvShowAsync(r.TvShow);
            _logger.Information($"TvShow {tvShow.Title} added to db");
            await _messageBus.Enqueue(new TvShowAddedToDbEvent(tvShow));
        }
    }
}
