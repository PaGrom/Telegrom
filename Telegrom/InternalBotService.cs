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
        private readonly IUpdateDispatcher _updateDispatcher;
        private readonly IRequestDispatcher _requestDispatcher;
        private readonly IWakeUpService _wakeUpService;
        private readonly ILogger<InternalBotService> _logger;

        public InternalBotService(SessionManager sessionManager,
            ITelegramUpdateReceiver telegramUpdateReceiver,
            IUpdateDispatcher updateDispatcher,
            IRequestDispatcher requestDispatcher,
            IWakeUpService wakeUpService,
            ILogger<InternalBotService> logger)
        {
            _sessionManager = sessionManager;
            _updateDispatcher = updateDispatcher;
            _requestDispatcher = requestDispatcher;
            _wakeUpService = wakeUpService;
            _logger = logger;

            telegramUpdateReceiver.SetUpdateReceivedHandler(HandleTelegramMessageReceived);
            telegramUpdateReceiver.Start();
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("InternalBotService started");
            _ = _updateDispatcher.RunAsync(cancellationToken);
            await _wakeUpService.WakeUpAsync(HandleTelegramMessageReceived, cancellationToken);
            await _requestDispatcher.RunAsync(cancellationToken);
            await _sessionManager.CompleteAllAsync();
        }

        public async void HandleTelegramMessageReceived(Update update, CancellationToken cancellationToken)
        {
            await _updateDispatcher.DispatchAsync(update, cancellationToken);
        }
    }
}
