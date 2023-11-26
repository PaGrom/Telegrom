using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegrom.Core;
using Telegrom.Core.Contexts;
using Telegrom.Core.MessageBus;

namespace Telegrom
{
    internal class InternalBotService
    {
        private readonly SessionManager _sessionManager;
        private readonly ITelegramUpdateReceiver _telegramUpdateReceiver;
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
            _telegramUpdateReceiver = telegramUpdateReceiver;
            _updateDispatcher = updateDispatcher;
            _requestDispatcher = requestDispatcher;
            _wakeUpService = wakeUpService;
            _logger = logger;
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("InternalBotService started");
            _telegramUpdateReceiver.SetUpdateReceivedHandler(HandleTelegramMessageReceived);
            _telegramUpdateReceiver.Start(cancellationToken);
            _ = _updateDispatcher.RunAsync(cancellationToken);
            await _wakeUpService.WakeUpAsync(HandleTelegramMessageReceived, cancellationToken);
            await _requestDispatcher.RunAsync(cancellationToken);
            await _sessionManager.CompleteAllAsync();
        }

        private Task HandleTelegramMessageReceived(Update update, CancellationToken cancellationToken)
        {
            return _updateDispatcher.DispatchAsync(update, cancellationToken);
        }
    }
}
