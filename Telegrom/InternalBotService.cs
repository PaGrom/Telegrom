using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegrom.Core;
using Telegrom.Core.Contexts;
using Telegrom.Core.MessageBus;
using Telegrom.Core.TelegramModel;

namespace Telegrom
{
    internal class InternalBotService
    {
        private readonly SessionManager _sessionManager;
        private readonly ITelegramUpdateDispatcher _telegramUpdateDispatcher;
        private readonly ITelegramRequestDispatcher _telegramRequestDispatcher;
        private readonly IWakeUpService _wakeUpService;
        private readonly ILogger<InternalBotService> _logger;

        public InternalBotService(SessionManager sessionManager,
            ITelegramUpdateReceiver telegramUpdateReceiver,
            ITelegramUpdateDispatcher telegramUpdateDispatcher,
            ITelegramRequestDispatcher telegramRequestDispatcher,
            IWakeUpService wakeUpService,
            ILogger<InternalBotService> logger)
        {
            _sessionManager = sessionManager;
            _telegramUpdateDispatcher = telegramUpdateDispatcher;
            _telegramRequestDispatcher = telegramRequestDispatcher;
            _wakeUpService = wakeUpService;
            _logger = logger;

            telegramUpdateReceiver.SetUpdateReceivedHandler(HandleTelegramMessageReceived);
            telegramUpdateReceiver.Start();
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("InternalBotService started");
            _ = _telegramUpdateDispatcher.RunAsync(cancellationToken);
            await _wakeUpService.WakeUpAsync(HandleTelegramMessageReceived, cancellationToken);
            await _telegramRequestDispatcher.RunAsync(cancellationToken);
            await _sessionManager.CompleteAllAsync();
        }

        public async void HandleTelegramMessageReceived(Update update, CancellationToken cancellationToken)
        {
            await _telegramUpdateDispatcher.DispatchAsync(update, cancellationToken);
        }
    }
}
