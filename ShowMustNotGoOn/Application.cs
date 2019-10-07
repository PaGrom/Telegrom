using System;
using System.Collections.Generic;
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
            _telegramService.SetCallbackButtonReceivedHandler(HandleCallbackButtonReceived);
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

        public async void HandleTelegramMessageReceived(UserMessage userMessage)
        {
            var channel = GetChannelForUser(userMessage.User.TelegramId);
            await channel.Writer.WriteAsync(new TelegramMessageReceivedEvent(userMessage));
        }

        private async void HandleCallbackButtonReceived(CallbackButton callbackButton)
        {
            var channel = GetChannelForUser(callbackButton.Message.User.TelegramId);
            await channel.Writer.WriteAsync(new TelegramCallbackButtonReceivedEvent(callbackButton));
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
                        try
                        {
                            await HandlerAsync(message);
                        }
                        catch (Exception e)
                        {
                            _logger.Error(e, $"Error inside handler for user {userId}");
                        }
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
                    await HandleMessageAsync(e.UserMessage);
                    break;
                case TelegramCallbackButtonReceivedEvent e:
                    await HandleCallbackButtonAsync(e.CallbackButton);
                    break;
            }
        }

        private async Task HandleCallbackButtonAsync(CallbackButton callbackButton)
        {
            var botMessage = callbackButton.Message;

            if (botMessage.BotCommandType == BotCommandType.Subscriptions)
            {
                await HandleSubscriptionsCallbackButtonAsync(callbackButton);
                return;
            }
                
            var tvShows = (await _tvShowsService.SearchTvShowsAsync(botMessage.SearchPattern)).ToList();

            switch (callbackButton.CallbackData)
            {
                case "next":
                    botMessage.CurrentPage++;
                    break;
                case "prev":
                    botMessage.CurrentPage--;
                    break;
                case "subendofshow":
                    await _usersService.SubscribeUserToTvShowAsync(botMessage.User,
                        tvShows[botMessage.CurrentPage],
                        SubscriptionType.EndOfShow);
                    break;
                case "unsubendofshow":
                    await _usersService.UnsubscribeUserFromTvShowAsync(botMessage.User,
                        tvShows[botMessage.CurrentPage],
                        SubscriptionType.EndOfShow);
                    break;
                default:
                    return;
            }

            botMessage.CurrentShowId = tvShows[botMessage.CurrentPage].MyShowsId;

            await _telegramService.UpdateMessageAsync(botMessage, callbackButton.CallbackId);
        }

        private async Task HandleSubscriptionsCallbackButtonAsync(CallbackButton callbackButton)
        {
            var botMessage = callbackButton.Message;

            var tvShows = callbackButton.Message.User.Subscriptions
                .Select(s => s.TvShow)
                .ToList();

            // handle navigate buttons
            if (callbackButton.CallbackData == "next"
                || callbackButton.CallbackData == "prev")
            {
                if (!tvShows.Any())
                {
                    await _telegramService.RemoveMessageAsync(botMessage);
                    await _telegramService.SendTextMessageToUserAsync(callbackButton.Message.User,
                        "You do not have any subscriptions yet");
                    return;
                }

                var currentPage = callbackButton.Message.CurrentPage;
                var currentShowId = callbackButton.Message.CurrentShowId;

                if (tvShows.Count <= currentPage
                    || tvShows[currentPage].MyShowsId != currentShowId)
                {
                    botMessage.CurrentPage = 0;
                    callbackButton.CallbackData = string.Empty;
                }

                if (callbackButton.CallbackData == "next")
                {
                    if (currentPage < tvShows.Count - 1)
                    {
                        botMessage.CurrentPage++;
                    }
                }

                if (callbackButton.CallbackData == "prev")
                {
                    if (currentPage > 0)
                    {
                        botMessage.CurrentPage--;
                    }
                }

                botMessage.CurrentShowId = tvShows[botMessage.CurrentPage].MyShowsId;
            }

            // handle subscribe button
            if (callbackButton.CallbackData == "subendofshow")
            {
                var tvShow = await _tvShowsService.GetTvShowAsync(botMessage.CurrentShowId);
                if (!botMessage.User.IsSubscribed(tvShow, SubscriptionType.EndOfShow))
                {
                    await _usersService.SubscribeUserToTvShowAsync(botMessage.User,
                        tvShow,
                        SubscriptionType.EndOfShow);
                }
            }

            // handle unsubscribe button
            if (callbackButton.CallbackData == "unsubendofshow")
            {
                var tvShow = await _tvShowsService.GetTvShowAsync(botMessage.CurrentShowId);
                if (botMessage.User.IsSubscribed(tvShow, SubscriptionType.EndOfShow))
                {
                    await _usersService.UnsubscribeUserFromTvShowAsync(botMessage.User,
                        tvShow,
                        SubscriptionType.EndOfShow);
                }
            }

            botMessage.TotalPages = tvShows.Count;

            await _telegramService.UpdateMessageAsync(botMessage, callbackButton.CallbackId);
        }

        private async Task HandleMessageAsync(UserMessage userMessage)
        {
            _logger.Information($"Received message from user {userMessage.User.Username}");

            if (userMessage.BotCommand != null)
            {
                await HandleBotCommandAsync(userMessage);
                return;
            }

            var searchPattern = userMessage.Text;
            const int pageCount = 0;

            var tvShows = (await _tvShowsService.SearchTvShowsAsync(searchPattern)).ToList();

            if (!tvShows.Any())
            {
                await _telegramService.SendTextMessageToUserAsync(userMessage.User, "Can't find tv show for you");
                return;
            }

            var botMessage = new BotMessage
            {
                User = userMessage.User,
                BotCommandType = null,
                SearchPattern = searchPattern,
                CurrentShowId = tvShows.First().MyShowsId,
                CurrentPage = pageCount,
                TotalPages = tvShows.Count
            };

            await _telegramService.SendMessageToUserAsync(userMessage.User, botMessage);
        }

        private async Task HandleBotCommandAsync(UserMessage userMessage)
        {
            switch (userMessage.BotCommand)
            {
                case BotCommandType.Start:
                    await _telegramService.SendTextMessageToUserAsync(userMessage.User, "Welcome");
                    break;
                case BotCommandType.Subscriptions:
                {
                    var tvShows = userMessage.User.Subscriptions
                        .Select(s => s.TvShow)
                        .ToList();

                    if (!tvShows.Any())
                    {
                        await _telegramService.SendTextMessageToUserAsync(userMessage.User, "You do not have any subscriptions yet");
                        break;
                    }

                    const int pageCount = 0;
                    var botMessage = new BotMessage
                    {
                        User = userMessage.User,
                        BotCommandType = BotCommandType.Subscriptions,
                        SearchPattern = null,
                        CurrentShowId = tvShows.First().MyShowsId,
                        CurrentPage = pageCount,
                        TotalPages = tvShows.Count
                    };

                    await _telegramService.SendMessageToUserAsync(userMessage.User, botMessage);
                    break;
                }
            }
        }
    }
}
