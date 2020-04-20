using System.Threading;
using System.Threading.Tasks;
using Serilog;
using ShowMustNotGoOn.Core;
using ShowMustNotGoOn.Core.Model;
using ShowMustNotGoOn.Messages.Events;

namespace ShowMustNotGoOn
{
    public class Application
    {
        private readonly SessionManager _sessionManager;
        private readonly ILogger _logger;

        public Application(SessionManager sessionManager,
            ITelegramMessageReceiver telegramMessageReceiver,
            ILogger logger)
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
            _logger.Information("Application start");
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
