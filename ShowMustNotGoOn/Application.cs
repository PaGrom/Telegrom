using System;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using ShowMustNotGoOn.Core;
using ShowMustNotGoOn.Core.MessageBus;
using ShowMustNotGoOn.Messages;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace ShowMustNotGoOn
{
    public class Application
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly ITvShowsRepository _tvShowsRepository;
        private readonly IMessageBus _messageBus;
        private readonly ILogger _logger;

        public Application(ITelegramBotClient telegramBotClient,
            ITvShowsRepository tvShowsRepository,
            IMessageBus messageBus,
            ILogger logger)
        {
            _telegramBotClient = telegramBotClient;
            _tvShowsRepository = tvShowsRepository;
            _messageBus = messageBus;
            _logger = logger;
        }

        public async Task RunAsync()
        {
            _logger.Information("Application start");

            _messageBus.RegisterHandler<RequestTvShow>(async r =>
            {
                var tvShows = await _tvShowsRepository.SearchTvShowsAsync(r.Name);
                _logger.Information($"Found {tvShows.Count()} by name {r.Name}");
            });

            await _messageBus.Enqueue(new RequestTvShow
            {
                Name = "Dark"
            });

            await Task.Delay(100000);

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
            _logger.Information("Get message {@message}", message);
        }
    }
}
