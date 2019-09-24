using System.Threading.Tasks;
using Serilog;
using ShowMustNotGoOn.Core;
using ShowMustNotGoOn.Core.MessageBus;
using ShowMustNotGoOn.Core.Model;
using ShowMustNotGoOn.Messages.Commands;
using ShowMustNotGoOn.Messages.Event;

namespace ShowMustNotGoOn
{
    public class Application
    {
        private readonly IMessageBus _messageBus;
        private readonly ITelegramService _telegramService;
        private readonly ILogger _logger;

        public Application(IMessageBus messageBus,
            ITelegramService telegramService,
            ILogger logger)
        {
            _messageBus = messageBus;
            _telegramService = telegramService;
            _logger = logger;

            _telegramService.SetMessageReceivedHandler(HandleTelegramMessageReceived);
            _telegramService.Start();

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

        public async void HandleTelegramMessageReceived(Message message)
        {
            await _messageBus.Enqueue(new TelegramMessageReceivedEvent(message));
        }
    }
}
