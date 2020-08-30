using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegrom.Core;

namespace Telegrom.TelegramService
{
    public class TelegramUpdateReceiver : ITelegramUpdateReceiver
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly ILogger<TelegramUpdateReceiver> _logger;
        private Action<Update, CancellationToken> _updateReceivedHandler;

        public TelegramUpdateReceiver(ITelegramBotClient telegramBotClient,
            ILogger<TelegramUpdateReceiver> logger)
        {
            _telegramBotClient = telegramBotClient;
            _logger = logger;

            var me = _telegramBotClient.GetMeAsync().GetAwaiter().GetResult();
            _logger.LogInformation($"Hello, World! I am identityUser {me.Id} and my name is {me.FirstName}.");
        }

        public void Start()
        {
            RegisterHandlers();
            _telegramBotClient.StartReceiving();
        }

        private void RegisterHandlers()
        {
            _telegramBotClient.OnUpdate += BotOnUpdateReceived;
        }

        private void BotOnUpdateReceived(object sender, UpdateEventArgs e)
        {
            var update = e.Update;
            _logger.LogInformation("Get update {@update}", update);

            _updateReceivedHandler?.Invoke(update, default);
        }

        public void SetUpdateReceivedHandler(Action<Update, CancellationToken> handler)
        {
            _updateReceivedHandler = handler;
        }

        public void Dispose()
        {
            _telegramBotClient.StopReceiving();
        }
    }
}
