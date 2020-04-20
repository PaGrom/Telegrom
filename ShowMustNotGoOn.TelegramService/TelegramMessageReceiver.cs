using System;
using System.Threading;
using AutoMapper;
using Serilog;
using ShowMustNotGoOn.Core;
using ShowMustNotGoOn.Core.Model;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace ShowMustNotGoOn.TelegramService
{
    public class TelegramMessageReceiver : ITelegramMessageReceiver
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private Action<UserMessage, CancellationToken> _messageReceivedHandler;
        private Action<UserCallback, CancellationToken> _callbackButtonReceivedHandler;

        public TelegramMessageReceiver(ITelegramBotClient telegramBotClient,
            IMapper mapper,
            ILogger logger)
        {
            _telegramBotClient = telegramBotClient;
            _mapper = mapper;
            _logger = logger;

            var me = _telegramBotClient.GetMeAsync().GetAwaiter().GetResult();
            _logger.Information($"Hello, World! I am user {me.Id} and my name is {me.FirstName}.");
        }

        public void Start()
        {
            RegisterHandlers();
            _telegramBotClient.StartReceiving();
        }

        private void RegisterHandlers()
        {
            _telegramBotClient.OnMessage += BotOnMessageReceived;
            _telegramBotClient.OnCallbackQuery += BotOnCallbackQueryReceived;
        }

        private void BotOnMessageReceived(object sender, MessageEventArgs e)
        {
            var telegramMessage = e.Message;
            _logger.Information("Get message {@message}", telegramMessage);
            var message = _mapper.Map<UserMessage>(telegramMessage);

            _messageReceivedHandler?.Invoke(message, default);
        }

        private void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs e)
        {
            var telegramCallback = e.CallbackQuery;
            _logger.Information("Get callback query {@callback}", telegramCallback);

            var callback = _mapper.Map<UserCallback>(telegramCallback);

            _callbackButtonReceivedHandler?.Invoke(callback, default);
        }

        public void SetMessageReceivedHandler(Action<UserMessage, CancellationToken> handler)
        {
            _messageReceivedHandler = handler;
        }

        public void SetCallbackButtonReceivedHandler(Action<UserCallback, CancellationToken> handler)
        {
            _callbackButtonReceivedHandler = handler;
        }

        public void Dispose()
        {
            _telegramBotClient.StopReceiving();
        }
    }
}
