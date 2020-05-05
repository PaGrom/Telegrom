using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ShowMustNotGoOn.Core.Request;
using ShowMustNotGoOn.Core.TelegramModel;
using ShowMustNotGoOn.DatabaseContext.Extensions;
using ShowMustNotGoOn.DatabaseContext.Model;

namespace ShowMustNotGoOn.Core.MessageBus
{
    public sealed class MessageHandler
    {
        private readonly RequestContext _requestContext;
        private readonly ITelegramService _telegramService;
        private readonly ITvShowsService _tvShowsService;
        private readonly DatabaseContext.DatabaseContext _databaseContext;
        private readonly ILogger<MessageHandler> _logger;

        public MessageHandler(RequestContext requestContext,
            ITelegramService telegramService,
            ITvShowsService tvShowsService,
            DatabaseContext.DatabaseContext databaseContext,
            ILogger<MessageHandler> logger)
        {
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

                switch (_requestContext.Update)
                {
                    case Message message:
                        await HandleMessageAsync(message, cancellationToken);
                        break;
                    case CallbackQuery callbackQuery:
                        await HandleCallbackAsync(callbackQuery, cancellationToken);
                        break;
                }

                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during message handling");
            }
        }

        private async Task HandleCallbackAsync(CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            var callbackId = Guid.Parse(callbackQuery.Data);

            var callback = await _databaseContext.Callbacks.FindAsync(new object[] {callbackId}, cancellationToken);

            var botMessage = await _databaseContext.BotMessages
                .FindAsync(new object[] { callback.BotMessageId }, cancellationToken);

            if (botMessage.BotCommandType == BotCommandType.Subscriptions)
            {
                await HandleSubscriptionsCommandAsync(callbackQuery, callback, botMessage, cancellationToken);
                return;
            }
            
            var messageText = await _databaseContext.MessageTexts
                .FindAsync(new object[] { botMessage.MessageTextId }, cancellationToken);

            var tvShows = (await _tvShowsService.SearchTvShowsAsync(messageText.Text, cancellationToken)).ToList();

            var currentShow = await _tvShowsService.GetTvShowByMyShowsIdAsync(botMessage.MyShowsId, cancellationToken)
                              ?? await _tvShowsService.GetTvShowFromMyShowsAsync(botMessage.MyShowsId, cancellationToken);

            switch (callback.CallbackType)
            {
                case CallbackType.Next:
                    botMessage.CurrentPage++;
                    break;
                case CallbackType.Prev:
                    botMessage.CurrentPage--;
                    break;
                case CallbackType.SubscribeToEndOfShow:
                    await _tvShowsService.SubscribeUserToTvShowAsync(_requestContext.SessionContext.User,
                        currentShow,
                        SubscriptionType.EndOfShow,
                        cancellationToken);
                    break;
                case CallbackType.UnsubscribeToEndOfShow:
                    await _tvShowsService.UnsubscribeUserFromTvShowAsync(_requestContext.SessionContext.User,
                        currentShow,
                        SubscriptionType.EndOfShow,
                        cancellationToken);
                    break;
                default:
                    return;
            }

            botMessage.MyShowsId = tvShows[botMessage.CurrentPage].Id;

            _databaseContext.BotMessages.Update(botMessage);
            await _databaseContext.SaveChangesAsync(cancellationToken);

            await _telegramService.UpdateMessageAsync(_requestContext.SessionContext.User,
                botMessage,
                callbackQuery.MessageId,
                callbackQuery.Id,
                cancellationToken);
        }

        private async Task HandleSubscriptionsCommandAsync(CallbackQuery callbackQuery, Callback callback, BotMessage botMessage, CancellationToken cancellationToken)
        {
            var subscriptions = await _tvShowsService.GetUserSubscriptionsAsync(_requestContext.SessionContext.User, cancellationToken);

            // handle navigate buttons
            if (callback.CallbackType == CallbackType.Next
                || callback.CallbackType == CallbackType.Prev)
            {
                if (!subscriptions.Any())
                {
                    // TODO: Do not remove message
                    await _telegramService.RemoveMessageAsync(_requestContext.SessionContext.User, callbackQuery.MessageId, cancellationToken);
                    _databaseContext.BotMessages.Remove(botMessage);
                    await _databaseContext.SaveChangesAsync(cancellationToken);
                    await _telegramService.SendTextMessageToUserAsync(_requestContext.SessionContext.User,
                        "You do not have any subscriptions yet", cancellationToken);
                    return;
                }

                var currentPage = botMessage.CurrentPage;

                if (subscriptions.Count <= currentPage)
                {
                    botMessage.CurrentPage = 0;
                    callbackQuery.Data = string.Empty;
                }

                switch (callback.CallbackType)
                {
                    case CallbackType.Next:
                    {
                        if (currentPage < subscriptions.Count - 1)
                        {
                            botMessage.CurrentPage++;
                        }

                        break;
                    }
                    case CallbackType.Prev:
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

            switch (callback.CallbackType)
            {
                // handle subscribe button
                case CallbackType.SubscribeToEndOfShow:
                {
                    var tvShow = await _tvShowsService.GetTvShowByMyShowsIdAsync(botMessage.MyShowsId, cancellationToken)
                                 ?? await _tvShowsService.GetTvShowFromMyShowsAsync(botMessage.MyShowsId, cancellationToken);
                    await _tvShowsService.SubscribeUserToTvShowAsync(_requestContext.SessionContext.User, tvShow, SubscriptionType.EndOfShow, cancellationToken);
                    break;
                }
                // handle unsubscribe button
                case CallbackType.UnsubscribeToEndOfShow:
                {
                    var tvShow = await _tvShowsService.GetTvShowByMyShowsIdAsync(botMessage.MyShowsId, cancellationToken)
                                 ?? await _tvShowsService.GetTvShowFromMyShowsAsync(botMessage.MyShowsId, cancellationToken);
                    await _tvShowsService.UnsubscribeUserFromTvShowAsync(_requestContext.SessionContext.User, tvShow, SubscriptionType.EndOfShow, cancellationToken);
                    break;
                }
            }

            botMessage.TotalPages = subscriptions.Count;

            _databaseContext.BotMessages.Update(botMessage);
            await _databaseContext.SaveChangesAsync(cancellationToken);

            await _telegramService.UpdateMessageAsync(_requestContext.SessionContext.User,
                botMessage,
                callbackQuery.MessageId,
                callbackQuery.Id,
                cancellationToken);
        }

        private async Task HandleMessageAsync(Message message, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Received message from identityUser {_requestContext.SessionContext.User.Username}");

            var messageTextString = message.Text.Trim();

            if (messageTextString.StartsWith("/"))
            {
                await HandleBotCommandAsync(message, cancellationToken);
                return;
            }

            const int pageCount = 0;

            var tvShows = (await _tvShowsService.SearchTvShowsAsync(messageTextString, cancellationToken)).ToList();

            if (!tvShows.Any())
            {
                await _telegramService.SendTextMessageToUserAsync(_requestContext.SessionContext.User, "Can't find tv show for you", cancellationToken);
                return;
            }

            var messageText = await _databaseContext.MessageTexts
                .AddIfNotExistsAsync(new MessageText
                    {
                        Text = messageTextString
                    }, s => s.Text == messageTextString, cancellationToken);

            await _databaseContext.SaveChangesAsync(cancellationToken);

            var botMessage = new BotMessage
            {
                UserId = _requestContext.SessionContext.User.Id,
                BotCommandType = null,
                MessageTextId = messageText.Id,
                MyShowsId = tvShows.First().Id,
                CurrentPage = pageCount,
                TotalPages = tvShows.Count
            };

            await _databaseContext.BotMessages.AddAsync(botMessage, cancellationToken);
            await _databaseContext.SaveChangesAsync(cancellationToken);

            await _telegramService.SendMessageToUserAsync(_requestContext.SessionContext.User, botMessage, cancellationToken);
        }

        private async Task HandleBotCommandAsync(Message message, CancellationToken cancellationToken)
        {
            var messageTextString = message.Text.Trim();

            var messageText = await _databaseContext.MessageTexts
                .AddIfNotExistsAsync(new MessageText
                    {
                        Text = messageTextString
                    }, s => s.Text == messageTextString, cancellationToken);

            await _databaseContext.SaveChangesAsync(cancellationToken);

            switch (messageTextString)
            {
                case "/start":
                    await _telegramService.SendTextMessageToUserAsync(_requestContext.SessionContext.User, "Welcome", cancellationToken);
                    break;
                case "/subscriptions":
                {
                    var subscriptions = await _tvShowsService.GetUserSubscriptionsAsync(_requestContext.SessionContext.User, cancellationToken);

                    if (!subscriptions.Any())
                    {
                        await _telegramService.SendTextMessageToUserAsync(_requestContext.SessionContext.User, "You do not have any subscriptions yet", cancellationToken);
                        break;
                    }

                    var show = await _tvShowsService.GetTvShowAsync(subscriptions.First().TvShowId, cancellationToken);

                    const int pageCount = 0;
                    var botMessage = new BotMessage
                    {
                        UserId = _requestContext.SessionContext.User.Id,
                        BotCommandType = BotCommandType.Subscriptions,
                        MessageTextId = messageText.Id,
                        MyShowsId = show.Id,
                        CurrentPage = pageCount,
                        TotalPages = subscriptions.Count
                    };

                    await _databaseContext.BotMessages.AddAsync(botMessage, cancellationToken);
                    await _databaseContext.SaveChangesAsync(cancellationToken);

                    await _telegramService.SendMessageToUserAsync(_requestContext.SessionContext.User, botMessage, cancellationToken);

                    break;
                }
            }
        }
    }
}
