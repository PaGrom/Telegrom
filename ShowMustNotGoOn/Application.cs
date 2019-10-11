using System.Threading.Tasks;
using Serilog;
using ShowMustNotGoOn.Core;
using ShowMustNotGoOn.Core.Model;
using ShowMustNotGoOn.Core.Model.Callback;
using ShowMustNotGoOn.Messages.Events;

namespace ShowMustNotGoOn
{
    public class Application
    {
        private readonly Dispatcher _dispatcher;
        private readonly ILogger _logger;

        public Application(Dispatcher dispatcher,
            ITelegramService telegramService,
            ILogger logger)
        {
            _dispatcher = dispatcher;
            _logger = logger;

            telegramService.SetMessageReceivedHandler(HandleTelegramMessageReceived);
            telegramService.SetCallbackButtonReceivedHandler(HandleCallbackButtonReceived);
            telegramService.Start();

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

        private async void HandleCallbackButtonReceived(CallbackButton callbackButton)
        {
            await _dispatcher.WriteAsync(new TelegramCallbackButtonReceivedEvent(callbackButton));
        }
    }
}
