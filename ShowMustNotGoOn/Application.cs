using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ShowMustNotGoOn.Core;
using ShowMustNotGoOn.Core.Session;
using ShowMustNotGoOn.Core.TelegramModel;

namespace ShowMustNotGoOn
{
    public class Application
    {
        private readonly SessionManager _sessionManager;
        private readonly ILogger<Application> _logger;

        public Application(SessionManager sessionManager,
            ITelegramUpdateReceiver telegramUpdateReceiver,
            ILogger<Application> logger)
        {
            _sessionManager = sessionManager;
            _logger = logger;

            telegramUpdateReceiver.SetUpdateReceivedHandler(HandleTelegramMessageReceived);
            telegramUpdateReceiver.Start();

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

        public async void HandleTelegramMessageReceived(Update update, CancellationToken cancellationToken)
        {
            var sessionContext = await _sessionManager.GetSessionContextAsync(update.From, cancellationToken);
            await sessionContext.PostUpdateAsync(update, cancellationToken);
        }
    }
}
