using System;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Serilog;
using ShowMustNotGoOn.Core;
using ShowMustNotGoOn.Core.MessageBus;
using ShowMustNotGoOn.Messages;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace ShowMustNotGoOn
{
    public class Application : IStartable
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly ITvShowsRepository _tvShowsRepository;
        private readonly IMessageBus _messageBus;
        private readonly IShowsDbRepository _dbRepository;
        private readonly ILogger _logger;

        public Application(ITelegramBotClient telegramBotClient,
            ITvShowsRepository tvShowsRepository,
            IMessageBus messageBus,
            IShowsDbRepository dbRepository,
            ILogger logger)
        {
            _telegramBotClient = telegramBotClient;
            _tvShowsRepository = tvShowsRepository;
            _messageBus = messageBus;
            _dbRepository = dbRepository;
            _logger = logger;
        }

        public async Task RunAsync()
        {
            _logger.Information("Application start");

            _messageBus.RegisterHandler<SaveTvShowToDb>(async r =>
            {
                var tvShow = await _dbRepository.AddNewTvShowAsync(r.TvShow);
            });

            _messageBus.RegisterHandler<RequestTvShow>(async r =>
            {
                var tvShows = await _tvShowsRepository.SearchTvShowsAsync(r.Name);
                var shows = tvShows.ToList();
                _logger.Information($"Found {shows.Count} by name {r.Name}");
                await _messageBus.Enqueue(new SaveTvShowToDb
                {
                    TvShow = shows.First()
                });
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

        public void Start()
        {
            RunAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}
