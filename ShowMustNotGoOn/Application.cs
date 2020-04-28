using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ShowMustNotGoOn.Core;
using ShowMustNotGoOn.Core.MessageBus.Events;
using ShowMustNotGoOn.Core.Session;
using ShowMustNotGoOn.DatabaseContext.Model;

namespace ShowMustNotGoOn
{
    public class Application
    {
        private readonly SessionManager _sessionManager;
        private readonly ILogger<Application> _logger;

        public Application(SessionManager sessionManager,
            ITelegramMessageReceiver telegramMessageReceiver,
            ILogger<Application> logger)
        {
            _sessionManager = sessionManager;
            _logger = logger;

            telegramMessageReceiver.SetMessageReceivedHandler(HandleTelegramMessageReceived);
            telegramMessageReceiver.SetCallbackButtonReceivedHandler(HandleCallbackButtonReceived);
            telegramMessageReceiver.Start();

            Task.Factory.StartNew(async () => { await RunAsync(); },
                TaskCreationOptions.LongRunning);
        }

        public async Task RunAsync()
        {
            _logger.LogInformation("Application start");
            while (true)
            {
                await Task.Delay(int.MaxValue);
            }
        }

        public async void HandleTelegramMessageReceived(UserMessage userMessage, CancellationToken cancellationToken)
        {
            var sessionContext = await _sessionManager.GetSessionContextAsync(userMessage.User, cancellationToken);
            await sessionContext.PostMessageAsync(new TelegramMessageReceivedEvent(userMessage), cancellationToken);
        }

        private async void HandleCallbackButtonReceived(UserCallback userCallback, CancellationToken cancellationToken)
        {
            var sessionContext = await _sessionManager.GetSessionContextAsync(userCallback.User, cancellationToken);
            await sessionContext.PostMessageAsync(new TelegramCallbackButtonReceivedEvent(userCallback), cancellationToken);
        }
    }
}
