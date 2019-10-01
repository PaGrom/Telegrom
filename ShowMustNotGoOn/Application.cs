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
using ShowMustNotGoOn.Core.Model.Callback;
using ShowMustNotGoOn.Core.Model.Callback.Navigate;
using ShowMustNotGoOn.Core.Model.Callback.Subscription;
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
                        await HandlerAsync(message);
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

        private async Task HandlerAsync(IMessage message)
        {
            switch (message)
            {
                case TelegramMessageReceivedEvent e:
                    await HandleMessageAsync(e.Message);
                    break;
                case TelegramCallbackQueryReceivedEvent e:
                    await HandleCallbackQueryAsync(e.CallbackQuery);
                    break;
            }
        }

        private async Task HandleCallbackQueryAsync(CallbackQuery callbackQuery)
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
                    await HandleNavigateCallbackQueryDataAsync(callbackQuery);
                    break;
                case SubscriptionCallbackQueryData _:
                    await HandleSubscriptionCallbackQueryDataAsync(callbackQuery);
                    break;
            }
        }

        private async Task HandleSubscriptionCallbackQueryDataAsync(CallbackQuery callbackQuery)
        {
            throw new NotImplementedException();
        }

        private async Task HandleNavigateCallbackQueryDataAsync(CallbackQuery callbackQuery)
        {
            var navigateCallbackQueryData = (NavigateCallbackQueryData) callbackQuery.CallbackQueryData;
            int pageCount;
            switch (navigateCallbackQueryData.CallbackQueryType)
            {
                case CallbackQueryType.NavigatePrev:
                    pageCount = navigateCallbackQueryData.PageCount - 1;
                    break;
                case CallbackQueryType.NavigateNext:
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
                prevButtonCallbackQueryData = CreatePrevButtonCallbackQueryData(user, callbackQuery.Message.MessageId, pageCount, searchPattern);
            }

            if (tvShows.Count > pageCount + 1)
            {
                nextButtonCallbackQueryData = CreateNextButtonCallbackQueryData(user, callbackQuery.Message.MessageId, pageCount, searchPattern);
            }

            var subscribeButtonCallbackQueryData = await _usersService.IsUserSubscribedToTvShowAsync(user, tvShow, SubscriptionType.EndOfShow)
                ? CreateUnsubscribeButtonCallbackQueryData(user, tvShow, callbackQuery.Message.MessageId)
                : CreateSubscribeButtonCallbackQueryData(user, tvShow, callbackQuery.Message.MessageId);

            await _databaseContext.SaveChangesAsync();
            await transaction.CommitAsync();

            await _telegramService.UpdateTvShowMessageAsync(callbackQuery.FromUser, tvShow, callbackQuery,
                prevButtonCallbackQueryData,
                nextButtonCallbackQueryData,
                subscribeButtonCallbackQueryData);
        }

        private async Task HandleMessageAsync(Message message)
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

            var tvShow = tvShows.First();

            ButtonCallbackQueryData nextButtonCallbackQueryData = null;

            await using var transaction = await _databaseContext.Database.BeginTransactionAsync();
            if (tvShows.Count > 1)
            {
                nextButtonCallbackQueryData = CreateNextButtonCallbackQueryData(user, default, pageCount, searchPattern);
            }

            var subscribeButtonCallbackQueryData = await _usersService.IsUserSubscribedToTvShowAsync(user, tvShow, SubscriptionType.EndOfShow)
                ? CreateUnsubscribeButtonCallbackQueryData(user, tvShow, default)
                : CreateSubscribeButtonCallbackQueryData(user, tvShow, default);

            await _databaseContext.SaveChangesAsync();

            var sentMessage = await _telegramService.SendTvShowToUserAsync(message.FromUser, tvShow,
                nextButtonCallbackQueryData,
                subscribeButtonCallbackQueryData);

            if (nextButtonCallbackQueryData != null)
            {
                nextButtonCallbackQueryData.MessageId = sentMessage.MessageId;
            }

            subscribeButtonCallbackQueryData.MessageId = sentMessage.MessageId;

            await _databaseContext.SaveChangesAsync();

            await transaction.CommitAsync();
        }

        private ButtonCallbackQueryData CreatePrevButtonCallbackQueryData(User user, int messageId, int pageCount, string searchPattern)
        {
            var prevNavigateCallbackQueryData = new PrevNavigateCallbackQueryData
            {
                PageCount = pageCount,
                SearchPattern = searchPattern
            };

            return _databaseContext.ButtonCallbackQueryDatas.Add(new ButtonCallbackQueryData
            {
                User = user,
                MessageId = messageId,
                Data = CallbackQueryDataSerializer.Serialize(prevNavigateCallbackQueryData)
            }).Entity;
        }

        private ButtonCallbackQueryData CreateNextButtonCallbackQueryData(User user, int messageId, int pageCount, string searchPattern)
        {
            var nextNavigateCallbackQueryData = new NextNavigateCallbackQueryData
            {
                PageCount = pageCount,
                SearchPattern = searchPattern
            };

            return _databaseContext.ButtonCallbackQueryDatas.Add(new ButtonCallbackQueryData
            {
                User = user,
                MessageId = messageId,
                CallbackQueryType = nextNavigateCallbackQueryData.CallbackQueryType,
                Data = CallbackQueryDataSerializer.Serialize(nextNavigateCallbackQueryData)
            }).Entity;
        }

        private ButtonCallbackQueryData CreateSubscribeButtonCallbackQueryData(User user, TvShow tvShow, int messageId)
        {
            var subscribeEndOfShow = new EndOfShowSubscriptionCallbackQueryData
            {
                TvShowId = tvShow.MyShowsId
            };

            return _databaseContext.ButtonCallbackQueryDatas.Add(new ButtonCallbackQueryData
            {
                User = user,
                MessageId = messageId,
                CallbackQueryType = subscribeEndOfShow.CallbackQueryType,
                Data = CallbackQueryDataSerializer.Serialize(subscribeEndOfShow)
            }).Entity;
        }

        private ButtonCallbackQueryData CreateUnsubscribeButtonCallbackQueryData(User user, TvShow tvShow, int messageId)
        {
            var unsubscribeEndOfShow = new EndOfShowUnsubscriptionCallbackQueryData
            {
                TvShowId = tvShow.MyShowsId
            };

            return _databaseContext.ButtonCallbackQueryDatas.Add(new ButtonCallbackQueryData
            {
                User = user,
                MessageId = messageId,
                CallbackQueryType = unsubscribeEndOfShow.CallbackQueryType,
                Data = CallbackQueryDataSerializer.Serialize(unsubscribeEndOfShow)
            }).Entity;
        }
    }
}
