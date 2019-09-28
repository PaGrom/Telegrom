using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Serilog;
using ShowMustNotGoOn.Core;
using ShowMustNotGoOn.Core.MessageBus;
using ShowMustNotGoOn.Core.Model;
using ShowMustNotGoOn.MessageBus;
using ShowMustNotGoOn.Messages.Event;

namespace ShowMustNotGoOn
{
    public class Application
    {
        private readonly ITelegramService _telegramService;
        private readonly IUsersService _usersService;
        private readonly ILogger _logger;

        private readonly LruChannelCollection _channelCollection = new LruChannelCollection(1);

        public Application(ITelegramService telegramService,
            IUsersService usersService,
            ILogger logger)
        {
            _telegramService = telegramService;
            _usersService = usersService;
            _logger = logger;

            _telegramService.SetMessageReceivedHandler(HandleTelegramMessageReceived);
            _telegramService.Start();

            Task.Factory.StartNew(async () => { await RunAsync(); },
                TaskCreationOptions.LongRunning);
        }

        public async Task RunAsync()
        {
            _logger.Information("Application start");
            await Task.Delay(5000);
            //await _messageBus.Enqueue(new SearchTvShowByNameCommand("Dark"));

            await Task.Delay(1000000);
        }

        public async void HandleTelegramMessageReceived(Message message)
        {
            var channel = GetChannelForUser(message.FromUser.TelegramId);
            await channel.Writer.WriteAsync(new TelegramMessageReceivedEvent(message));
        }

        private Channel<IMessage> GetChannelForUser(int userId)
        {
            var channelExists = _channelCollection.TryGetChannel(userId, out var channel);
            if (channelExists)
            {
                return channel;
            }

            channel = Channel.CreateUnbounded<IMessage>();

            var cancellationTokenSource = new CancellationTokenSource();

            var task = Task.Factory.StartNew(async () =>
            {
                while (await channel.Reader.WaitToReadAsync(cancellationTokenSource.Token))
                {
                    var message = (TelegramMessageReceivedEvent)(await channel.Reader.ReadAsync(cancellationTokenSource.Token));
                    await Handler(message.Message);
                }
            }, cancellationTokenSource.Token);

            _channelCollection.Add(userId, channel, task, cancellationTokenSource);

            return channel;
        }

        private async Task Handler(Message message)
        {
            _logger.Information($"Received message from user {message.FromUser.Username}");
            await _usersService.AddOrUpdateUserAsync(message.FromUser);
            if (message.BotCommand == BotCommandType.Start)
            {
                await _telegramService.SendWelcomeMessageToUser(message.FromUser);
            }
        }
    }
}
