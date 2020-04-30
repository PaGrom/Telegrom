using System;
using System.Linq;
using System.Threading;
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

        public async Task HandleAsync(CancellationToken cancellationToken)
        {
            try
            {
                await using var transaction = await _databaseContext.Database.BeginTransactionAsync(cancellationToken);

                switch (_requestContext.Message)
                {
                    case TelegramMessageReceivedEvent e:
                        await HandleMessageAsync(e.UserMessage, cancellationToken);
                        break;
                    case TelegramCallbackButtonReceivedEvent e:
                        await HandleCallbackButtonAsync(e.UserCallback, cancellationToken);
                        break;
                }

                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during message handling");
            }
        }

        private async Task HandleCallbackButtonAsync(UserCallback userCallback, CancellationToken cancellationToken)
        {
            var botMessage = await _databaseContext.BotMessages
                .SingleAsync(m => _sessionContext.User.TelegramId == userCallback.User.TelegramId
                                  && m.MessageId == userCallback.MessageId, cancellationToken: cancellationToken);

            var callbackButton = new CallbackButton
            {
                Message = botMessage,
                CallbackId = userCallback.CallbackId,
                CallbackData = userCallback.CallbackData
            };

            if (botMessage.BotCommandType == BotCommandType.Subscriptions)
            {
                await HandleSubscriptionsCallbackButtonAsync(callbackButton, cancellationToken);
                return;
            }
                
            var tvShows = (await _tvShowsService.SearchTvShowsAsync(botMessage.SearchPattern, cancellationToken)).ToList();

            var currentShow = await _tvShowsService.GetTvShowByMyShowsIdAsync(botMessage.MyShowsId, cancellationToken)
                              ?? await _tvShowsService.GetTvShowFromMyShowsAsync(botMessage.MyShowsId, cancellationToken);

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
                        SubscriptionType.EndOfShow,
                        cancellationToken);
                    break;
                case "unsubendofshow":
                    await _tvShowsService.UnsubscribeUserFromTvShowAsync(_sessionContext.User,
                        currentShow,
                        SubscriptionType.EndOfShow,
                        cancellationToken);
                    break;
                default:
                    return;
            }

            botMessage.MyShowsId = tvShows[botMessage.CurrentPage].MyShowsId;

            botMessage = await _telegramService.UpdateMessageAsync(_sessionContext.User, botMessage, callbackButton.CallbackId, cancellationToken);

            _databaseContext.BotMessages.Update(botMessage);
            await _databaseContext.SaveChangesAsync(cancellationToken);
        }

        private async Task HandleSubscriptionsCallbackButtonAsync(CallbackButton callbackButton, CancellationToken cancellationToken)
        {
            var botMessage = callbackButton.Message;

            var subscriptions = await _tvShowsService.GetUserSubscriptionsAsync(_sessionContext.User, cancellationToken);

            // handle navigate buttons
            if (callbackButton.CallbackData == "next"
                || callbackButton.CallbackData == "prev")
            {
                if (!subscriptions.Any())
                {
                    await _telegramService.RemoveMessageAsync(_sessionContext.User, botMessage, cancellationToken);
                    _databaseContext.BotMessages.Remove(botMessage);
                    await _databaseContext.SaveChangesAsync(cancellationToken);
                    await _telegramService.SendTextMessageToUserAsync(_sessionContext.User,
                        "You do not have any subscriptions yet", cancellationToken);
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

                var show = await _tvShowsService.GetTvShowAsync(subscriptions[botMessage.CurrentPage].TvShowId, cancellationToken);
                botMessage.MyShowsId = show.MyShowsId;
            }

            switch (callbackButton.CallbackData)
            {
                // handle subscribe button
                case "subendofshow":
                {
                    var tvShow = await _tvShowsService.GetTvShowByMyShowsIdAsync(botMessage.MyShowsId, cancellationToken)
                                 ?? await _tvShowsService.GetTvShowFromMyShowsAsync(botMessage.MyShowsId, cancellationToken);
                    await _tvShowsService.SubscribeUserToTvShowAsync(_sessionContext.User, tvShow, SubscriptionType.EndOfShow, cancellationToken);
                    break;
                }
                // handle unsubscribe button
                case "unsubendofshow":
                {
                    var tvShow = await _tvShowsService.GetTvShowByMyShowsIdAsync(botMessage.MyShowsId, cancellationToken)
                                 ?? await _tvShowsService.GetTvShowFromMyShowsAsync(botMessage.MyShowsId, cancellationToken);
                    await _tvShowsService.UnsubscribeUserFromTvShowAsync(_sessionContext.User, tvShow, SubscriptionType.EndOfShow, cancellationToken);
                    break;
                }
            }

            botMessage.TotalPages = subscriptions.Count;

            botMessage = await _telegramService.UpdateMessageAsync(_sessionContext.User, botMessage, callbackButton.CallbackId, cancellationToken);

            _databaseContext.BotMessages.Update(botMessage);
            await _databaseContext.SaveChangesAsync(cancellationToken);
        }

        private async Task HandleMessageAsync(UserMessage userMessage, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Received message from user {userMessage.User.Username}");

            if (userMessage.BotCommand != null)
            {
                await HandleBotCommandAsync(userMessage, cancellationToken);
                return;
            }

            var searchPattern = userMessage.Text;
            const int pageCount = 0;

            var tvShows = (await _tvShowsService.SearchTvShowsAsync(searchPattern, cancellationToken)).ToList();

            if (!tvShows.Any())
            {
                await _telegramService.SendTextMessageToUserAsync(userMessage.User, "Can't find tv show for you", cancellationToken);
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

            botMessage = await _telegramService.SendMessageToUserAsync(userMessage.User, botMessage, cancellationToken);

            await _databaseContext.BotMessages.AddAsync(botMessage, cancellationToken);
            await _databaseContext.SaveChangesAsync(cancellationToken);
        }

        private async Task HandleBotCommandAsync(UserMessage userMessage, CancellationToken cancellationToken)
        {
            switch (userMessage.BotCommand)
            {
                case BotCommandType.Start:
                    await _telegramService.SendTextMessageToUserAsync(userMessage.User, "Welcome", cancellationToken);
                    break;
                case BotCommandType.Subscriptions:
                {
                    var subscriptions = await _tvShowsService.GetUserSubscriptionsAsync(userMessage.User, cancellationToken);

                    if (!subscriptions.Any())
                    {
                        await _telegramService.SendTextMessageToUserAsync(userMessage.User, "You do not have any subscriptions yet", cancellationToken);
                        break;
                    }

                    var show = await _tvShowsService.GetTvShowAsync(subscriptions.First().TvShowId, cancellationToken);

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

                    botMessage = await _telegramService.SendMessageToUserAsync(userMessage.User, botMessage, cancellationToken);

                    await _databaseContext.BotMessages.AddAsync(botMessage, cancellationToken);
                    await _databaseContext.SaveChangesAsync(cancellationToken);

                    break;
                }
            }
        }
    }
}
