using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShowMustNotGoOn.Core.Request;
using ShowMustNotGoOn.Core.Session;
using ShowMustNotGoOn.DatabaseContext.Model;
using Telegram.Bot.Types.Enums;

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

                switch (_requestContext.Update.Type)
                {
                    case UpdateType.Message:
                        await HandleMessageAsync(cancellationToken);
                        break;
                    case UpdateType.CallbackQuery:
                        await HandleCallbackButtonAsync(cancellationToken);
                        break;
                }

                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during message handling");
            }
        }

        private async Task HandleCallbackButtonAsync(CancellationToken cancellationToken)
        {
            var botMessage = await _databaseContext.BotMessages
                .SingleAsync(m => m.UserId == _sessionContext.User.Id
                                  && m.MessageId == _requestContext.Update.CallbackQuery.Message.MessageId,
                    cancellationToken);

            if (botMessage.BotCommandType == BotCommandType.Subscriptions)
            {
                await HandleSubscriptionsCallbackButtonAsync(botMessage, cancellationToken);
                return;
            }
                
            var tvShows = (await _tvShowsService.SearchTvShowsAsync(botMessage.SearchPattern, cancellationToken)).ToList();

            var currentShow = await _tvShowsService.GetTvShowByMyShowsIdAsync(botMessage.MyShowsId, cancellationToken)
                              ?? await _tvShowsService.GetTvShowFromMyShowsAsync(botMessage.MyShowsId, cancellationToken);

            switch (_requestContext.Update.CallbackQuery.Data)
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

            botMessage.MyShowsId = tvShows[botMessage.CurrentPage].Id;

            botMessage = await _telegramService.UpdateMessageAsync(_sessionContext.User, botMessage, _requestContext.Update.CallbackQuery.Id, cancellationToken);

            _databaseContext.BotMessages.Update(botMessage);
            await _databaseContext.SaveChangesAsync(cancellationToken);
        }

        private async Task HandleSubscriptionsCallbackButtonAsync(BotMessage botMessage, CancellationToken cancellationToken)
        {
            var subscriptions = await _tvShowsService.GetUserSubscriptionsAsync(_sessionContext.User, cancellationToken);

            // handle navigate buttons
            if (_requestContext.Update.CallbackQuery.Data == "next"
                || _requestContext.Update.CallbackQuery.Data == "prev")
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

                var currentPage = botMessage.CurrentPage;

                if (subscriptions.Count <= currentPage)
                {
                    botMessage.CurrentPage = 0;
                    _requestContext.Update.CallbackQuery.Data = string.Empty;
                }

                switch (_requestContext.Update.CallbackQuery.Data)
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
                botMessage.MyShowsId = show.Id;
            }

            switch (_requestContext.Update.CallbackQuery.Data)
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

            botMessage = await _telegramService.UpdateMessageAsync(_sessionContext.User, botMessage, _requestContext.Update.CallbackQuery.Id, cancellationToken);

            _databaseContext.BotMessages.Update(botMessage);
            await _databaseContext.SaveChangesAsync(cancellationToken);
        }

        private async Task HandleMessageAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Received message from identityUser {_sessionContext.User.Username}");

            if (_requestContext.Update.Message.Text.Trim().StartsWith("/"))
            {
                await HandleBotCommandAsync(cancellationToken);
                return;
            }

            var searchPattern = _requestContext.Update.Message.Text.Trim();
            const int pageCount = 0;

            var tvShows = (await _tvShowsService.SearchTvShowsAsync(searchPattern, cancellationToken)).ToList();

            if (!tvShows.Any())
            {
                await _telegramService.SendTextMessageToUserAsync(_sessionContext.User, "Can't find tv show for you", cancellationToken);
                return;
            }

            var botMessage = new BotMessage
            {
                UserId = _sessionContext.User.Id,
                BotCommandType = null,
                SearchPattern = searchPattern,
                MyShowsId = tvShows.First().Id,
                CurrentPage = pageCount,
                TotalPages = tvShows.Count
            };

            botMessage = await _telegramService.SendMessageToUserAsync(_sessionContext.User, botMessage, cancellationToken);

            await _databaseContext.BotMessages.AddAsync(botMessage, cancellationToken);
            await _databaseContext.SaveChangesAsync(cancellationToken);
        }

        private async Task HandleBotCommandAsync(CancellationToken cancellationToken)
        {
            switch (_requestContext.Update.Message.Text.Trim())
            {
                case "/start":
                    await _telegramService.SendTextMessageToUserAsync(_sessionContext.User, "Welcome", cancellationToken);
                    break;
                case "/subscriptions":
                {
                    var subscriptions = await _tvShowsService.GetUserSubscriptionsAsync(_sessionContext.User, cancellationToken);

                    if (!subscriptions.Any())
                    {
                        await _telegramService.SendTextMessageToUserAsync(_sessionContext.User, "You do not have any subscriptions yet", cancellationToken);
                        break;
                    }

                    var show = await _tvShowsService.GetTvShowAsync(subscriptions.First().TvShowId, cancellationToken);

                    const int pageCount = 0;
                    var botMessage = new BotMessage
                    {
                        UserId = _sessionContext.User.Id,
                        BotCommandType = BotCommandType.Subscriptions,
                        SearchPattern = null,
                        MyShowsId = show.Id,
                        CurrentPage = pageCount,
                        TotalPages = subscriptions.Count
                    };

                    botMessage = await _telegramService.SendMessageToUserAsync(_sessionContext.User, botMessage, cancellationToken);

                    await _databaseContext.BotMessages.AddAsync(botMessage, cancellationToken);
                    await _databaseContext.SaveChangesAsync(cancellationToken);

                    break;
                }
            }
        }
    }
}
