using System;
using System.Linq;
using System.Text.RegularExpressions;
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
        private readonly ITvShowsService _tvShowsService;
        private readonly ILogger _logger;
        private readonly Regex _callbackQueryRegex = new Regex(@"^(?<searchPattern>[^@]+)@(?<pageCount>\d+)@(?<command>[^@]+)$");

        private readonly LruChannelCollection _channelCollection = new LruChannelCollection(1);

        public Application(ITelegramService telegramService,
            IUsersService usersService,
            ITvShowsService tvShowsService,
            ILogger logger)
        {
            _telegramService = telegramService;
            _usersService = usersService;
            _tvShowsService = tvShowsService;
            _logger = logger;

            _telegramService.SetMessageReceivedHandler(HandleTelegramMessageReceived);
            _telegramService.SetCallbackQueryReceivedHandler(HandleCallbackQueryReceived);
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

        private async void HandleCallbackQueryReceived(CallbackQuery callbackQuery)
        {
            var channel = GetChannelForUser(callbackQuery.FromUser.TelegramId);
            await channel.Writer.WriteAsync(new TelegramCallbackQueryReceivedEvent(callbackQuery));
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
                try
                {
                    while (await channel.Reader.WaitToReadAsync(cancellationTokenSource.Token))
                    {
                        var message = await channel.Reader.ReadAsync(cancellationTokenSource.Token);
                        await Handler(message);
                    }
                }
                catch (OperationCanceledException e)
                {
                    _logger.Information("Operation cancelled");
                }
            }, cancellationTokenSource.Token);

            _channelCollection.Add(userId, channel, task, cancellationTokenSource);

            return channel;
        }

        private async Task Handler(IMessage message)
        {
            switch (message)
            {
                case TelegramMessageReceivedEvent e:
                    await HandleMessage(e.Message);
                    break;
                case TelegramCallbackQueryReceivedEvent e:
                    await HandleCallbackQuery(e.CallbackQuery);
                    break;
            }
        }

        private async Task HandleCallbackQuery(CallbackQuery callbackQuery)
        {
            _logger.Information($"Received callback query from user {callbackQuery.FromUser.TelegramId}");
            var match = _callbackQueryRegex.Match(callbackQuery.Data);
            if (!match.Success)
            {
                _logger.Error($"Can't parse callback query data: {callbackQuery.Data}");
                return;
            }

            var searchPattern = match.Groups["searchPattern"].Value;
            var pageCount = int.Parse(match.Groups["pageCount"].Value);
            var command = match.Groups["command"].Value;

            if (command == "next")
            {
                pageCount++;
                var tvShows = (await _tvShowsService.SearchTvShowsAsync(searchPattern)).ToList();
                var callBackData = $"{searchPattern}@{pageCount}";
                var nextCallbackQueryData = $"{callBackData}@next";
                var prevCallbackQueryData = $"{callBackData}@prev";
                if (tvShows.Count == pageCount + 1)
                {
                    nextCallbackQueryData = null;
                }
                var tvShow = (await _tvShowsService.SearchTvShowsAsync(searchPattern)).Skip(pageCount).FirstOrDefault();
                await _telegramService.UpdateTvShowMessage(callbackQuery.FromUser, tvShow, callbackQuery.Message.MessageId, nextCallbackQueryData, prevCallbackQueryData);
                return;
            }

            if (command == "prev")
            {
                pageCount--;
                var tvShows = (await _tvShowsService.SearchTvShowsAsync(searchPattern)).ToList();
                var callBackData = $"{searchPattern}@{pageCount}";
                var nextCallbackQueryData = $"{callBackData}@next";
                var prevCallbackQueryData = $"{callBackData}@prev";
                if (pageCount == 0)
                {
                    prevCallbackQueryData = null;
                }
                var tvShow = (await _tvShowsService.SearchTvShowsAsync(searchPattern)).Skip(pageCount).FirstOrDefault();
                await _telegramService.UpdateTvShowMessage(callbackQuery.FromUser, tvShow, callbackQuery.Message.MessageId, nextCallbackQueryData, prevCallbackQueryData);
                return;
            }
        }

        private async Task HandleMessage(Message message)
        {
            _logger.Information($"Received message from user {message.FromUser.Username}");
            await _usersService.AddOrUpdateUserAsync(message.FromUser);
            if (message.BotCommand == BotCommandType.Start)
            {
                await _telegramService.SendTextMessageToUser(message.FromUser, "Welcome");
                return;
            }

            var searchPattern = message.Text.Replace('@', ' ');
            var pageCount = 0;

            var tvShows = (await _tvShowsService.SearchTvShowsAsync(searchPattern)).ToList();

            if (!tvShows.Any())
            {
                await _telegramService.SendTextMessageToUser(message.FromUser, "Can't find tv show for you");
                return;
            }

            string nextCallbackQueryData = null;

            if (tvShows.Count > 1)
            {
                nextCallbackQueryData = $"{searchPattern}@{pageCount}@next";
            }

            await _telegramService.SendTvShowToUser(message.FromUser, tvShows.First(), nextCallbackQueryData);
        }
    }
}
