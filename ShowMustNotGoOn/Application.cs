using System;
using System.Threading.Tasks;
using Serilog;
using ShowMustNotGoOn.Core;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace ShowMustNotGoOn
{
    public class Application
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly ITvShowsRepository _tvShowsRepository;
        private readonly ILogger _logger;

        public Application(ITelegramBotClient telegramBotClient, ITvShowsRepository tvShowsRepository, ILogger logger)
        {
            _telegramBotClient = telegramBotClient;
            _tvShowsRepository = tvShowsRepository;
            _logger = logger;
        }

        public async Task RunAsync()
        {
            var data = await _tvShowsRepository.SearchTvShowsAsync("Dark");

            _logger.Information("Application start");
            var me = await _telegramBotClient.GetMeAsync();
            _logger.Information($"Hello, World! I am user {me.Id} and my name is {me.FirstName}.");

            _telegramBotClient.OnMessage += BotOnMessageReceived;

            _telegramBotClient.StartReceiving();
            Console.ReadLine();
            _telegramBotClient.StopReceiving();
        }

        private void BotOnMessageReceived(object sender, MessageEventArgs e)
        {
            var message = e.Message;
            //_logger.Information($"Get message from user {message.Contact.FirstName} {message.Contact.LastName} ({message.Contact.UserId}) Text: ");
            _logger.Information("Get message {@message}", message);
        }
    }
}
