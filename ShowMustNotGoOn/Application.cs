using System.Threading.Tasks;
using Serilog;
using ShowMustNotGoOn.Core;
using ShowMustNotGoOn.Core.Model;
using ShowMustNotGoOn.Messages.Events;

namespace ShowMustNotGoOn
{
    public class Application
    {
        private readonly Dispatcher _dispatcher;
        private readonly ILogger _logger;

        public Application(Dispatcher dispatcher,
            ITelegramMessageReceiver telegramMessageReceiver,
            ILogger logger)
        {
            _dispatcher = dispatcher;
            _logger = logger;

            telegramMessageReceiver.SetMessageReceivedHandler(HandleTelegramMessageReceived);
            telegramMessageReceiver.SetCallbackButtonReceivedHandler(HandleCallbackButtonReceived);
            telegramMessageReceiver.Start();

            Task.Factory.StartNew(async () => { await RunAsync(); },
                TaskCreationOptions.LongRunning);
        }

        public async Task RunAsync()
        {
            _logger.Information("Application start");
            while (true)
            {
                await Task.Delay(int.MaxValue);
            }
        }

        public async void HandleTelegramMessageReceived(UserMessage userMessage)
        {
            await _dispatcher.WriteAsync(new TelegramMessageReceivedEvent(userMessage));
        }

        private async void HandleCallbackButtonReceived(UserCallback userCallback)
        {
            await _dispatcher.WriteAsync(new TelegramCallbackButtonReceivedEvent(userCallback));
        }
    }
}
