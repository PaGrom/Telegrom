using System;
using System.Threading;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegrom.Core;
using Telegrom.Core.TelegramModel;

namespace Telegrom.TelegramService
{
    public class TelegramUpdateReceiver : ITelegramUpdateReceiver
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly IMapper _mapper;
        private readonly ILogger<TelegramUpdateReceiver> _logger;
        private Action<Update, CancellationToken> _updateReceivedHandler;

        public TelegramUpdateReceiver(ITelegramBotClient telegramBotClient,
            IMapper mapper,
            ILogger<TelegramUpdateReceiver> logger)
        {
            _telegramBotClient = telegramBotClient;
            _mapper = mapper;
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

            Update telegramUpdate = update.Type switch
            {
                UpdateType.Message => _mapper.Map<Message>(update),
                UpdateType.CallbackQuery => _mapper.Map<CallbackQuery>(update),
                _ => null
            };

            if (telegramUpdate != null)
            {
                _updateReceivedHandler?.Invoke(telegramUpdate, default);
            }
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
