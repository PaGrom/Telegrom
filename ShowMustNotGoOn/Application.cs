using System.Threading.Tasks;
using Serilog;
using ShowMustNotGoOn.Core.MessageBus;
using ShowMustNotGoOn.Messages.Commands;

namespace ShowMustNotGoOn
{
    public class Application
    {
        private readonly IMessageBus _messageBus;
        private readonly ILogger _logger;

        public Application(IMessageBus messageBus,
            ILogger logger)
        {
            _messageBus = messageBus;
            _logger = logger;

            Task.Factory.StartNew(async () => { await RunAsync(); },
                TaskCreationOptions.LongRunning);
        }

        public async Task RunAsync()
        {
            _logger.Information("Application start");
            await Task.Delay(1000);
            await _messageBus.Enqueue(new SearchTvShowByNameCommand("Dark"));

            await Task.Delay(1000000);
        }
    }
}
