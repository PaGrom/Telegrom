using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegrom.Core;

namespace Telegrom.TelegramService
{
    public class TelegramUpdateReceiver : ITelegramUpdateReceiver, IDisposable
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly ILogger<TelegramUpdateReceiver> _logger;
        private Func<Update, CancellationToken, Task>? _updateReceivedHandler;
        private CancellationTokenSource _cts;

        public TelegramUpdateReceiver(ITelegramBotClient telegramBotClient,
            ILogger<TelegramUpdateReceiver> logger)
        {
            _telegramBotClient = telegramBotClient;
            _logger = logger;

            var me = _telegramBotClient.GetMeAsync().GetAwaiter().GetResult();
            _logger.LogInformation($"Hello, World! I am identityUser {me.Id} and my name is {me.FirstName}.");
        }
        
        public void SetUpdateReceivedHandler(Func<Update, CancellationToken, Task> handler)
        {
            _updateReceivedHandler = handler;
        }

        public void Start(CancellationToken cancellationToken)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var receiverOptions = new ReceiverOptions();
            
            _telegramBotClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, _cts.Token);
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Get update {@update}", update);

            if (_updateReceivedHandler != null)
            {
                await _updateReceivedHandler(update, cancellationToken);
            }
        }
        
        private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            _logger.LogError("An error occurred in receiving updates: {Exception}", exception);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _cts.Cancel();
            _logger.LogInformation("Receiver stopped");
        }
    }
}
