using System;
using Serilog;
using ShowMustNotGoOn.Core;
using ShowMustNotGoOn.Core.MessageBus;
using ShowMustNotGoOn.Messages.Event;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace ShowMustNotGoOn
{
    public class TelegramService : ITelegramService, IDisposable
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly IMessageBus _messageBus;
        private readonly ILogger _logger;

        public TelegramService(ITelegramBotClient telegramBotClient,
            IMessageBus messageBus,
            ILogger logger)
        {
            _telegramBotClient = telegramBotClient;
            _messageBus = messageBus;
            _logger = logger;

            var me = _telegramBotClient.GetMeAsync().GetAwaiter().GetResult();
            _logger.Information($"Hello, World! I am user {me.Id} and my name is {me.FirstName}.");

            RegisterHandlers();

            _telegramBotClient.StartReceiving();
        }

        private void RegisterHandlers()
        {
            _telegramBotClient.OnMessage += BotOnMessageReceived;
        }

        private void BotOnMessageReceived(object sender, MessageEventArgs e)
        {
            var message = e.Message;
            _logger.Information("Get message {@message}", message);
            _messageBus.Enqueue(new TelegramMessageReceivedEvent(message));
        }

        public void Dispose()
        {
            _telegramBotClient.StopReceiving();
        }
    }
}
