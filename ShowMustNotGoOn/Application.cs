using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegrom.Core;
using Telegrom.Core.Contexts;
using Telegrom.Core.MessageBus;
using Telegrom.Core.TelegramModel;

namespace ShowMustNotGoOn
{
    public class Application : IDisposable
    {
	    private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private readonly SessionManager _sessionManager;
        private readonly ITelegramRequestDispatcher _telegramRequestDispatcher;
        private readonly ILogger<Application> _logger;

        public Application(SessionManager sessionManager,
            ITelegramUpdateReceiver telegramUpdateReceiver,
            ITelegramRequestDispatcher telegramRequestDispatcher,
            ILogger<Application> logger)
        {
            _sessionManager = sessionManager;
            _telegramRequestDispatcher = telegramRequestDispatcher;
            _logger = logger;

            telegramUpdateReceiver.SetUpdateReceivedHandler(HandleTelegramMessageReceived);
            telegramUpdateReceiver.Start();

            Task.Factory.StartNew(async () => { await RunAsync(); },
                TaskCreationOptions.LongRunning);
        }

        public async Task RunAsync()
        {
            _logger.LogInformation("Application start");
            await _telegramRequestDispatcher.RunAsync(_cancellationTokenSource.Token);
            await _sessionManager.CompleteAllAsync();
        }

        public async void HandleTelegramMessageReceived(Update update, CancellationToken cancellationToken)
        {
            var sessionContext = await _sessionManager.GetSessionContextAsync(update.From, cancellationToken);
            await sessionContext.PostUpdateAsync(update, cancellationToken);
        }

        public void Dispose()
        {
	        _cancellationTokenSource?.Dispose();
        }
    }
}
