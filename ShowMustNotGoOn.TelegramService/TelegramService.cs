using System;
using System.Threading.Tasks;
using AutoMapper;
using Serilog;
using ShowMustNotGoOn.Core;
using ShowMustNotGoOn.Core.Model;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace ShowMustNotGoOn.TelegramService
{
    public class TelegramService : ITelegramService, IDisposable
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private Action<Message> _messageReceivedHandler;

        public TelegramService(ITelegramBotClient telegramBotClient,
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
        }

        public void SetMessageReceivedHandler(Action<Message> handler)
        {
            _messageReceivedHandler = handler;
        }

        private void BotOnMessageReceived(object sender, MessageEventArgs e)
        {
            var message = e.Message;
            _logger.Information("Get message {@message}", message);
            _messageReceivedHandler?.Invoke(_mapper.Map<Message>(message));
        }

        public async Task SendWelcomeMessageToUser(User user)
        {
            await _telegramBotClient.SendTextMessageAsync(user.TelegramId, "Welcome");
        } 

        public void Dispose()
        {
            _telegramBotClient.StopReceiving();
        }
    }
}
