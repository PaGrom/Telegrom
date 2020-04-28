using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShowMustNotGoOn.Core.MessageBus.Events;
using ShowMustNotGoOn.Core.Request;
using ShowMustNotGoOn.Core.Session;
using ShowMustNotGoOn.DatabaseContext.Model;
using ShowMustNotGoOn.DatabaseContext.Model.Callback;

namespace ShowMustNotGoOn.Core.MessageBus
{
    public sealed class MessageHandler
    {
        private readonly SessionContext _sessionContext;
        private readonly RequestContext _requestContext;
        private readonly ITelegramService _telegramService;
        private readonly ITvShowsService _tvShowsService;
        private readonly DatabaseContext.DatabaseContext _databaseContext;
        private readonly ILogger<MessageHandler> _logger;

        public MessageHandler(SessionContext sessionContext,
            RequestContext requestContext,
            ITelegramService telegramService,
            ITvShowsService tvShowsService,
            DatabaseContext.DatabaseContext databaseContext,
            ILogger<MessageHandler> logger)
        {
            _sessionContext = sessionContext;
            _requestContext = requestContext;
            _telegramService = telegramService;
            _tvShowsService = tvShowsService;
            _databaseContext = databaseContext;
            _logger = logger;
        }

        public async Task HandleAsync()
        {
            try
            {
                await using var transaction = await _databaseContext.Database.BeginTransactionAsync();

                switch (_requestContext.Message)
                {
                    case TelegramMessageReceivedEvent e:
                        await HandleMessageAsync(e.UserMessage);
                        break;
                    case TelegramCallbackButtonReceivedEvent e:
                        await HandleCallbackButtonAsync(e.UserCallback);
                        break;
                }

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during message handling");
            }
        }

        private async Task HandleCallbackButtonAsync(UserCallback userCallback)
        {
            var botMessage = await _databaseContext.BotMessages
                .SingleAsync(m => _sessionContext.User.TelegramId == userCallback.User.TelegramId
                                  && m.MessageId == userCallback.MessageId);

            var callbackButton = new CallbackButton
            {
                Message = botMessage,
                CallbackId = userCallback.CallbackId,
                CallbackData = userCallback.CallbackData
            };

            if (botMessage.BotCommandType == BotCommandType.Subscriptions)
            {
                await HandleSubscriptionsCallbackButtonAsync(callbackButton);
                return;
            }
                
            var tvShows = (await _tvShowsService.SearchTvShowsAsync(botMessage.SearchPattern)).ToList();

            var currentShow = await _tvShowsService.GetTvShowByMyShowsIdAsync(botMessage.MyShowsId)
                              ?? await _tvShowsService.GetTvShowFromMyShowsAsync(botMessage.MyShowsId);

            switch (callbackButton.CallbackData)
            {
                case "next":
                    botMessage.CurrentPage++;
                    break;
                case "prev":
                    botMessage.CurrentPage--;
                    break;
                case "subendofshow":
                    await _tvShowsService.SubscribeUserToTvShowAsync(_sessionContext.User,
                        currentShow,
                        SubscriptionType.EndOfShow);
                    break;
                case "unsubendofshow":
                    await _tvShowsService.UnsubscribeUserFromTvShowAsync(_sessionContext.User,
                        currentShow,
                        SubscriptionType.EndOfShow);
                    break;
                default:
                    return;
            }

            botMessage.MyShowsId = tvShows[botMessage.CurrentPage].MyShowsId;

            botMessage = await _telegramService.UpdateMessageAsync(_sessionContext.User, botMessage, callbackButton.CallbackId);

            _databaseContext.BotMessages.Update(botMessage);
            await _databaseContext.SaveChangesAsync();
        }

        private async Task HandleSubscriptionsCallbackButtonAsync(CallbackButton callbackButton)
        {
            var botMessage = callbackButton.Message;

            var subscriptions = await _tvShowsService.GetUserSubscriptionsAsync(_sessionContext.User);

            // handle navigate buttons
            if (callbackButton.CallbackData == "next"
                || callbackButton.CallbackData == "prev")
            {
                if (!subscriptions.Any())
                {
                    await _telegramService.RemoveMessageAsync(_sessionContext.User, botMessage);
                    _databaseContext.BotMessages.Remove(botMessage);
                    await _databaseContext.SaveChangesAsync();
                    await _telegramService.SendTextMessageToUserAsync(_sessionContext.User,
                        "You do not have any subscriptions yet");
                    return;
                }

                var currentPage = callbackButton.Message.CurrentPage;

                if (subscriptions.Count <= currentPage)
                {
                    botMessage.CurrentPage = 0;
                    callbackButton.CallbackData = string.Empty;
                }

                switch (callbackButton.CallbackData)
                {
                    case "next":
                    {
                        if (currentPage < subscriptions.Count - 1)
                        {
                            botMessage.CurrentPage++;
                        }

                        break;
                    }
                    case "prev":
                    {
                        if (currentPage > 0)
                        {
                            botMessage.CurrentPage--;
                        }

                        break;
                    }
                }

                var show = await _tvShowsService.GetTvShowAsync(subscriptions[botMessage.CurrentPage].TvShowId);
                botMessage.MyShowsId = show.MyShowsId;
            }

            switch (callbackButton.CallbackData)
            {
                // handle subscribe button
                case "subendofshow":
                {
                    var tvShow = await _tvShowsService.GetTvShowByMyShowsIdAsync(botMessage.MyShowsId)
                                 ?? await _tvShowsService.GetTvShowFromMyShowsAsync(botMessage.MyShowsId);
                    await _tvShowsService.SubscribeUserToTvShowAsync(_sessionContext.User, tvShow, SubscriptionType.EndOfShow);
                    break;
                }
                // handle unsubscribe button
                case "unsubendofshow":
                {
                    var tvShow = await _tvShowsService.GetTvShowByMyShowsIdAsync(botMessage.MyShowsId)
                                 ?? await _tvShowsService.GetTvShowFromMyShowsAsync(botMessage.MyShowsId);
                    await _tvShowsService.UnsubscribeUserFromTvShowAsync(_sessionContext.User, tvShow, SubscriptionType.EndOfShow);
                    break;
                }
            }

            botMessage.TotalPages = subscriptions.Count;

            botMessage = await _telegramService.UpdateMessageAsync(_sessionContext.User, botMessage, callbackButton.CallbackId);

            _databaseContext.BotMessages.Update(botMessage);
            await _databaseContext.SaveChangesAsync();
        }

        private async Task HandleMessageAsync(UserMessage userMessage)
        {
            _logger.LogInformation($"Received message from user {userMessage.User.Username}");

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
                UserId = userMessage.User.Id,
                BotCommandType = null,
                SearchPattern = searchPattern,
                MyShowsId = tvShows.First().MyShowsId,
                CurrentPage = pageCount,
                TotalPages = tvShows.Count
            };

            botMessage = await _telegramService.SendMessageToUserAsync(userMessage.User, botMessage);

            await _databaseContext.BotMessages.AddAsync(botMessage);
            await _databaseContext.SaveChangesAsync();
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
                    var subscriptions = await _tvShowsService.GetUserSubscriptionsAsync(userMessage.User);

                    if (!subscriptions.Any())
                    {
                        await _telegramService.SendTextMessageToUserAsync(userMessage.User, "You do not have any subscriptions yet");
                        break;
                    }

                    var show = await _tvShowsService.GetTvShowAsync(subscriptions.First().TvShowId);

                    const int pageCount = 0;
                    var botMessage = new BotMessage
                    {
                        UserId = userMessage.User.Id,
                        BotCommandType = BotCommandType.Subscriptions,
                        SearchPattern = null,
                        MyShowsId = show.MyShowsId,
                        CurrentPage = pageCount,
                        TotalPages = subscriptions.Count
                    };

                    botMessage = await _telegramService.SendMessageToUserAsync(userMessage.User, botMessage);

                    await _databaseContext.BotMessages.AddAsync(botMessage);
                    await _databaseContext.SaveChangesAsync();

                    break;
                }
            }
        }
    }
}
