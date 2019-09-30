using System;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Serilog;
using ShowMustNotGoOn.Core;
using ShowMustNotGoOn.Core.MessageBus;
using ShowMustNotGoOn.Core.Model;
using ShowMustNotGoOn.Core.Model.CallbackQuery;
using ShowMustNotGoOn.Messages.Events;

namespace ShowMustNotGoOn
{
    public class Application
    {
        private readonly ITelegramService _telegramService;
        private readonly IUsersService _usersService;
        private readonly ITvShowsService _tvShowsService;
        private readonly DatabaseContext.DatabaseContext _databaseContext;
        private readonly ILogger _logger;

        private readonly LruChannelCollection _channelCollection = new LruChannelCollection(1);

        public Application(ITelegramService telegramService,
            IUsersService usersService,
            ITvShowsService tvShowsService,
            DatabaseContext.DatabaseContext databaseContext,
            ILogger logger)
        {
            _telegramService = telegramService;
            _usersService = usersService;
            _tvShowsService = tvShowsService;
            _databaseContext = databaseContext;
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
            while (true)
            {
                await Task.Delay(1000000);
            }
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

            var callbackQueryDataString = (await _databaseContext.ButtonCallbackQueryDatas
                .SingleOrDefaultAsync(c => c.User.TelegramId == callbackQuery.FromUser.TelegramId
                                           && c.Id == callbackQuery.CallbackQueryDataId))
                ?.Data;

            if (string.IsNullOrEmpty(callbackQueryDataString))
            {
                _logger.Error($"Can't find callback query data");
                return;
            }

            callbackQuery.CallbackQueryData = CallbackQueryDataSerializer.Deserialize(callbackQueryDataString);

            switch (callbackQuery.CallbackQueryData)
            {
                case NavigateCallbackQueryData _:
                    await HandleNavigateCallbackQueryData(callbackQuery);
                    break;
            }
        }

        private async Task HandleNavigateCallbackQueryData(CallbackQuery callbackQuery)
        {
            var navigateCallbackQueryData = (NavigateCallbackQueryData) callbackQuery.CallbackQueryData;
            int pageCount;
            switch (navigateCallbackQueryData.CallbackQueryType)
            {
                case CallbackQueryType.Prev:
                    pageCount = navigateCallbackQueryData.PageCount - 1;
                    break;
                case CallbackQueryType.Next:
                    pageCount = navigateCallbackQueryData.PageCount + 1;
                    break;
                default:
                    _logger.Error($"Something go wrong with NavigateCallbackQueryData");
                    return;
            }

            var searchPattern = navigateCallbackQueryData.SearchPattern;

            var tvShows = (await _tvShowsService.SearchTvShowsAsync(searchPattern)).ToList();
            var tvShow = tvShows.Skip(pageCount).FirstOrDefault();

            await using var transaction = await _databaseContext.Database.BeginTransactionAsync();

            var user = await _databaseContext.Users.SingleOrDefaultAsync(u =>
                u.TelegramId == callbackQuery.FromUser.TelegramId);

            if (user == null)
            {
                _logger.Error($"Can't find user with Telegram Id {callbackQuery.FromUser.TelegramId} in db");
                return;
            }

            object[] existedCallbackQueryDatasForMessage = await _databaseContext.ButtonCallbackQueryDatas
                .Where(c => c.User.TelegramId == callbackQuery.FromUser.TelegramId
                            && c.MessageId == callbackQuery.Message.MessageId)
                .ToArrayAsync();

            _databaseContext.RemoveRange(existedCallbackQueryDatasForMessage);

            ButtonCallbackQueryData prevButtonCallbackQueryData = null;
            ButtonCallbackQueryData nextButtonCallbackQueryData = null;

            if (pageCount > 0)
            {
                var prevNavigateCallbackQueryData = new PrevNavigateCallbackQueryData
                {
                    PageCount = pageCount,
                    SearchPattern = searchPattern
                };

                prevButtonCallbackQueryData = _databaseContext.ButtonCallbackQueryDatas.Add(new ButtonCallbackQueryData
                {
                    User = user,
                    MessageId = callbackQuery.Message.MessageId,
                    Data = CallbackQueryDataSerializer.Serialize(prevNavigateCallbackQueryData)
                }).Entity;
            }

            if (tvShows.Count > pageCount + 1)
            {
                var nextNavigateCallbackQueryData = new NextNavigateCallbackQueryData
                {
                    PageCount = pageCount,
                    SearchPattern = searchPattern
                };

                nextButtonCallbackQueryData = _databaseContext.ButtonCallbackQueryDatas.Add(new ButtonCallbackQueryData
                {
                    User = user,
                    MessageId = callbackQuery.Message.MessageId,
                    Data = CallbackQueryDataSerializer.Serialize(nextNavigateCallbackQueryData)
                }).Entity;
            }

            await _databaseContext.SaveChangesAsync();
            await transaction.CommitAsync();

            var prevNavigateCallbackQueryDataId = prevButtonCallbackQueryData?.Id;
            var nextNavigateCallbackQueryDataId = nextButtonCallbackQueryData?.Id;

            await _telegramService.UpdateTvShowMessage(callbackQuery.FromUser, tvShow, callbackQuery,
                prevNavigateCallbackQueryDataId, nextNavigateCallbackQueryDataId);
        }

        private async Task HandleMessage(Message message)
        {
            _logger.Information($"Received message from user {message.FromUser.Username}");
            var user = await _usersService.AddOrUpdateUserAsync(message.FromUser);
            if (message.BotCommand == BotCommandType.Start)
            {
                await _telegramService.SendTextMessageToUser(message.FromUser, "Welcome");
                return;
            }

            var searchPattern = message.Text;
            var pageCount = 0;

            var tvShows = (await _tvShowsService.SearchTvShowsAsync(searchPattern)).ToList();

            if (!tvShows.Any())
            {
                await _telegramService.SendTextMessageToUser(message.FromUser, "Can't find tv show for you");
                return;
            }

            ButtonCallbackQueryData nextButtonCallbackQueryData = null;

            await using var transaction = await _databaseContext.Database.BeginTransactionAsync();
            if (tvShows.Count > 1)
            {
                
                var nextNavigateCallbackQueryData = new NextNavigateCallbackQueryData
                {
                    PageCount = pageCount,
                    SearchPattern = searchPattern
                };

                nextButtonCallbackQueryData = _databaseContext.ButtonCallbackQueryDatas.Add(new ButtonCallbackQueryData
                {
                    User = user,
                    Data = CallbackQueryDataSerializer.Serialize(nextNavigateCallbackQueryData)
                }).Entity;

                await _databaseContext.SaveChangesAsync();
            }

            var nextNavigateCallbackQueryDataId = nextButtonCallbackQueryData?.Id;

            var sentMessage = await _telegramService.SendTvShowToUser(message.FromUser, tvShows.First(), nextNavigateCallbackQueryDataId);

            if (nextButtonCallbackQueryData != null)
            {
                nextButtonCallbackQueryData.MessageId = sentMessage.MessageId;
                await _databaseContext.SaveChangesAsync();
            }

            await transaction.CommitAsync();
        }
    }
}
