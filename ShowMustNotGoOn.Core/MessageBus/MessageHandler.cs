//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Logging;
//using ShowMustNotGoOn.Core.Contexts;
//using ShowMustNotGoOn.Core.TelegramModel;
//using ShowMustNotGoOn.DatabaseContext.Extensions;
//using ShowMustNotGoOn.DatabaseContext.Model;

//namespace ShowMustNotGoOn.Core.MessageBus
//{
//    public sealed class MessageHandler
//    {
//	    private const string NotFoundImage = "https://images-na.ssl-images-amazon.com/images/I/312yeogBelL._SX466_.jpg";

//        private readonly IUpdateContext _updateContext;
//        private readonly ITvShowsService _tvShowsService;
//        private readonly DatabaseContext.DatabaseContext _databaseContext;
//        private readonly ILogger<MessageHandler> _logger;

//        public MessageHandler(IUpdateContext updateContext,
//            ITvShowsService tvShowsService,
//            DatabaseContext.DatabaseContext databaseContext,
//            ILogger<MessageHandler> logger)
//        {
//            _updateContext = updateContext;
//            _tvShowsService = tvShowsService;
//            _databaseContext = databaseContext;
//            _logger = logger;
//        }

//        public async Task UpdateHandleAsync(CancellationToken cancellationToken)
//        {
//            try
//            {
//                switch (_updateContext.Update)
//                {
//                    case Message message:
//                        await HandleMessageAsync(message, cancellationToken);
//                        break;
//                    case CallbackQuery callbackQuery:
//                        await HandleCallbackAsync(callbackQuery, cancellationToken);
//                        break;
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error during message handling");
//            }
//        }

//        private async Task HandleCallbackAsync(CallbackQuery callbackQuery, CancellationToken cancellationToken)
//        {
//            var callbackId = Guid.Parse(callbackQuery.Data);

//            var callback = await _databaseContext.Callbacks.FindAsync(new object[] {callbackId}, cancellationToken);

//            var botMessage = await _databaseContext.BotMessages
//                .FindAsync(new object[] { callback.BotMessageId }, cancellationToken);

//            if (botMessage.BotCommandType == BotCommandType.Subscriptions)
//            {
//                await HandleSubscriptionsCommandAsync(callbackQuery, callback, botMessage, cancellationToken);
//                return;
//            }
            
//            var messageText = await _databaseContext.MessageTexts
//                .FindAsync(new object[] { botMessage.MessageTextId }, cancellationToken);

//            var tvShows = (await _tvShowsService.SearchTvShowsAsync(messageText.Text, cancellationToken)).ToList();

//            var currentShow = await _tvShowsService.GetTvShowByMyShowsIdAsync(botMessage.MyShowsId, cancellationToken)
//                              ?? await _tvShowsService.GetTvShowFromMyShowsAsync(botMessage.MyShowsId, cancellationToken);

//            switch (callback.CallbackType)
//            {
//                case CallbackType.Next:
//                    botMessage.CurrentPage++;
//                    break;
//                case CallbackType.Prev:
//                    botMessage.CurrentPage--;
//                    break;
//                case CallbackType.SubscribeToEndOfShow:
//                    await _tvShowsService.SubscribeUserToTvShowAsync(_updateContext.SessionContext.User,
//                        currentShow,
//                        SubscriptionType.EndOfShow,
//                        cancellationToken);
//                    break;
//                case CallbackType.UnsubscribeToEndOfShow:
//                    await _tvShowsService.UnsubscribeUserFromTvShowAsync(_updateContext.SessionContext.User,
//                        currentShow,
//                        SubscriptionType.EndOfShow,
//                        cancellationToken);
//                    break;
//                default:
//                    return;
//            }

//            botMessage.MyShowsId = tvShows[botMessage.CurrentPage].Id;

//            _databaseContext.BotMessages.Update(botMessage);
//            await _databaseContext.SaveChangesAsync(cancellationToken);

//            await UpdateMessageAsync(_updateContext.SessionContext.User,
//                botMessage,
//                callbackQuery.MessageId,
//                callbackQuery.Id,
//                cancellationToken);
//        }

//        private async Task HandleSubscriptionsCommandAsync(CallbackQuery callbackQuery, Callback callback, BotMessage botMessage, CancellationToken cancellationToken)
//        {
//            var subscriptions = await _tvShowsService.GetUserSubscriptionsAsync(_updateContext.SessionContext.User, cancellationToken);

//            // handle navigate buttons
//            if (callback.CallbackType == CallbackType.Next
//                || callback.CallbackType == CallbackType.Prev)
//            {
//                if (!subscriptions.Any())
//                {
//                    // TODO: Do not remove message
//                    await RemoveMessageAsync(_updateContext.SessionContext.User, callbackQuery.MessageId, cancellationToken);
//                    _databaseContext.BotMessages.Remove(botMessage);
//                    await _databaseContext.SaveChangesAsync(cancellationToken);
//                    await SendTextMessageToUserAsync(_updateContext.SessionContext.User,
//                        "You do not have any subscriptions yet", cancellationToken);
//                    return;
//                }

//                var currentPage = botMessage.CurrentPage;

//                if (subscriptions.Count <= currentPage)
//                {
//                    botMessage.CurrentPage = 0;
//                    callbackQuery.Data = string.Empty;
//                }

//                switch (callback.CallbackType)
//                {
//                    case CallbackType.Next:
//                    {
//                        if (currentPage < subscriptions.Count - 1)
//                        {
//                            botMessage.CurrentPage++;
//                        }

//                        break;
//                    }
//                    case CallbackType.Prev:
//                    {
//                        if (currentPage > 0)
//                        {
//                            botMessage.CurrentPage--;
//                        }

//                        break;
//                    }
//                }

//                var show = await _tvShowsService.GetTvShowAsync(subscriptions[botMessage.CurrentPage].TvShowId, cancellationToken);
//                botMessage.MyShowsId = show.Id;
//            }

//            switch (callback.CallbackType)
//            {
//                // handle subscribe button
//                case CallbackType.SubscribeToEndOfShow:
//                {
//                    var tvShow = await _tvShowsService.GetTvShowByMyShowsIdAsync(botMessage.MyShowsId, cancellationToken)
//                                 ?? await _tvShowsService.GetTvShowFromMyShowsAsync(botMessage.MyShowsId, cancellationToken);
//                    await _tvShowsService.SubscribeUserToTvShowAsync(_updateContext.SessionContext.User, tvShow, SubscriptionType.EndOfShow, cancellationToken);
//                    break;
//                }
//                // handle unsubscribe button
//                case CallbackType.UnsubscribeToEndOfShow:
//                {
//                    var tvShow = await _tvShowsService.GetTvShowByMyShowsIdAsync(botMessage.MyShowsId, cancellationToken)
//                                 ?? await _tvShowsService.GetTvShowFromMyShowsAsync(botMessage.MyShowsId, cancellationToken);
//                    await _tvShowsService.UnsubscribeUserFromTvShowAsync(_updateContext.SessionContext.User, tvShow, SubscriptionType.EndOfShow, cancellationToken);
//                    break;
//                }
//            }

//            botMessage.TotalPages = subscriptions.Count;

//            _databaseContext.BotMessages.Update(botMessage);
//            await _databaseContext.SaveChangesAsync(cancellationToken);

//            await UpdateMessageAsync(_updateContext.SessionContext.User,
//                botMessage,
//                callbackQuery.MessageId,
//                callbackQuery.Id,
//                cancellationToken);
//        }

//        private async Task HandleMessageAsync(Message message, CancellationToken cancellationToken)
//        {
//            _logger.LogInformation($"Received message from identityUser {_updateContext.SessionContext.User.Username}");

//            var messageTextString = message.Text.Trim();

//            if (messageTextString.StartsWith("/"))
//            {
//                await HandleBotCommandAsync(message, cancellationToken);
//                return;
//            }

//            const int pageCount = 0;

//            var tvShows = (await _tvShowsService.SearchTvShowsAsync(messageTextString, cancellationToken)).ToList();

//            if (!tvShows.Any())
//            {
//                await SendTextMessageToUserAsync(_updateContext.SessionContext.User, "Can't find tv show for you", cancellationToken);
//                return;
//            }

//            var messageText = await _databaseContext.MessageTexts
//                .AddIfNotExistsAsync(new MessageText
//                    {
//                        Text = messageTextString
//                    }, s => s.Text == messageTextString, cancellationToken);

//            await _databaseContext.SaveChangesAsync(cancellationToken);

//            var botMessage = new BotMessage
//            {
//                UserId = _updateContext.SessionContext.User.Id,
//                BotCommandType = null,
//                MessageTextId = messageText.Id,
//                MyShowsId = tvShows.First().Id,
//                CurrentPage = pageCount,
//                TotalPages = tvShows.Count
//            };

//            await _databaseContext.BotMessages.AddAsync(botMessage, cancellationToken);
//            await _databaseContext.SaveChangesAsync(cancellationToken);

//            await SendMessageToUserAsync(_updateContext.SessionContext.User, botMessage, cancellationToken);
//        }

//        private async Task HandleBotCommandAsync(Message message, CancellationToken cancellationToken)
//        {
//            var messageTextString = message.Text.Trim();

//            var messageText = await _databaseContext.MessageTexts
//                .AddIfNotExistsAsync(new MessageText
//                    {
//                        Text = messageTextString
//                    }, s => s.Text == messageTextString, cancellationToken);

//            await _databaseContext.SaveChangesAsync(cancellationToken);

//            switch (messageTextString)
//            {
//                case "/start":
//                    await SendTextMessageToUserAsync(_updateContext.SessionContext.User, "Welcome", cancellationToken);
//                    break;
//                case "/subscriptions":
//                {
//                    var subscriptions = await _tvShowsService.GetUserSubscriptionsAsync(_updateContext.SessionContext.User, cancellationToken);

//                    if (!subscriptions.Any())
//                    {
//                        await SendTextMessageToUserAsync(_updateContext.SessionContext.User, "You do not have any subscriptions yet", cancellationToken);
//                        break;
//                    }

//                    var show = await _tvShowsService.GetTvShowAsync(subscriptions.First().TvShowId, cancellationToken);

//                    const int pageCount = 0;
//                    var botMessage = new BotMessage
//                    {
//                        UserId = _updateContext.SessionContext.User.Id,
//                        BotCommandType = BotCommandType.Subscriptions,
//                        MessageTextId = messageText.Id,
//                        MyShowsId = show.Id,
//                        CurrentPage = pageCount,
//                        TotalPages = subscriptions.Count
//                    };

//                    await _databaseContext.BotMessages.AddAsync(botMessage, cancellationToken);
//                    await _databaseContext.SaveChangesAsync(cancellationToken);

//                    await SendMessageToUserAsync(_updateContext.SessionContext.User, botMessage, cancellationToken);

//                    break;
//                }
//            }
//        }

//        public Task SendTextMessageToUserAsync(User user, string text, CancellationToken cancellationToken)
//        {
//            var request = new SendMessageRequest(user.Id, text);
//            return _updateContext.SessionContext.PostRequestAsync(request, cancellationToken);
//        }

//        public async Task SendMessageToUserAsync(User user, BotMessage message, CancellationToken cancellationToken)
//        {
//            var show = await _tvShowsService.GetTvShowByMyShowsIdAsync(message.MyShowsId, cancellationToken)
//                       ?? await _tvShowsService.GetTvShowFromMyShowsAsync(message.MyShowsId, cancellationToken);

//            if (string.IsNullOrEmpty(show.Image))
//            {
//                show.Image = NotFoundImage;
//            }

//            var buttons = await GetButtonsAsync(user, message, show, cancellationToken);
//            var markup = new InlineKeyboardMarkup(buttons);

//            var request = new SendPhotoRequest(user.Id, show.Image)
//            {
//                Caption = $"{show.Title} / {show.TitleOriginal}",
//                ReplyMarkup = markup
//            };

//            await _updateContext.SessionContext.PostRequestAsync(request, cancellationToken);
//        }

//        public async Task UpdateMessageAsync(User user, BotMessage message, int telegramMessageId, string callbackId, CancellationToken cancellationToken)
//        {
//            var oldCallbacks = await _databaseContext.Callbacks
//                .Where(c => c.BotMessageId == message.Id)
//                .ToListAsync(cancellationToken);

//            _databaseContext.Callbacks.RemoveRange(oldCallbacks);

//            var show = await _tvShowsService.GetTvShowByMyShowsIdAsync(message.MyShowsId, cancellationToken)
//                       ?? await _tvShowsService.GetTvShowFromMyShowsAsync(message.MyShowsId, cancellationToken);

//            if (string.IsNullOrEmpty(show.Image))
//            {
//                show.Image = NotFoundImage;
//            }

//            var buttons = await GetButtonsAsync(user, message, show, cancellationToken);
//            var markup = new InlineKeyboardMarkup(buttons);

//            var answerCallbackQueryRequest = new AnswerCallbackQueryRequest(callbackId);

//            await _updateContext.SessionContext.PostRequestAsync(answerCallbackQueryRequest, cancellationToken);

//            var editMessageMediaRequest = new EditMessageMediaRequest(user.Id, telegramMessageId, show.Image);

//            await _updateContext.SessionContext.PostRequestAsync(editMessageMediaRequest, cancellationToken);

//            var editCaptionRequest = new EditMessageCaptionRequest(user.Id, telegramMessageId, $"{ show.Title } / { show.TitleOriginal}")
//            {
//                ReplyMarkup = markup
//            };

//            await _updateContext.SessionContext.PostRequestAsync(editCaptionRequest, cancellationToken);
//        }

//        public Task RemoveMessageAsync(User user, int messageId, CancellationToken cancellationToken)
//        {
//            var deleteMessageRequest = new DeleteMessageRequest(user.Id, messageId);
//            return _updateContext.SessionContext.PostRequestAsync(deleteMessageRequest, cancellationToken);
//        }

//        private async Task<List<List<InlineKeyboardButton>>> GetButtonsAsync(User user, BotMessage message, TvShow show, CancellationToken cancellationToken)
//        {
//            var buttons = new List<List<InlineKeyboardButton>>
//            {
//                await GetNavigateButtonsAsync(message, cancellationToken)
//            };

//            var subscription = await _tvShowsService.GetUserSubscriptionToTvShowAsync(user, show, SubscriptionType.EndOfShow, cancellationToken);

//            if (subscription != null)
//            {
//                var callback = await CreateCallbackAsync(message.Id, CallbackType.UnsubscribeToEndOfShow, cancellationToken);
//                buttons.Add(new List<InlineKeyboardButton>
//                {
//                    InlineKeyboardButton.WithCallbackData("Unsubscribe from end of show", callback.Id.ToString())
//                });
//            }
//            else
//            {
//                var callback = await CreateCallbackAsync(message.Id, CallbackType.SubscribeToEndOfShow, cancellationToken);
//                buttons.Add(new List<InlineKeyboardButton>
//                {
//                    InlineKeyboardButton.WithCallbackData("Subscribe to end of show", callback.Id.ToString())
//                });
//            }

//            return buttons;
//        }

//        private async Task<List<InlineKeyboardButton>> GetNavigateButtonsAsync(BotMessage message, CancellationToken cancellationToken)
//        {
//            var buttons = new List<InlineKeyboardButton>();

//            if (message.CurrentPage > 0)
//            {
//                var callback = await CreateCallbackAsync(message.Id, CallbackType.Prev, cancellationToken);
//                buttons.Add(InlineKeyboardButton.WithCallbackData("Prev", callback.Id.ToString()));
//            }

//            if (message.CurrentPage < message.TotalPages - 1)
//            {
//                var callback = await CreateCallbackAsync(message.Id, CallbackType.Next, cancellationToken);
//                buttons.Add(InlineKeyboardButton.WithCallbackData("Next", callback.Id.ToString()));
//            }

//            return buttons;
//        }

//        private async Task<Callback> CreateCallbackAsync(int botMessageId, CallbackType callbackType, CancellationToken cancellationToken)
//        {
//            var callback = (await _databaseContext.Callbacks
//                .AddAsync(new Callback
//                {
//                    BotMessageId = botMessageId,
//                    CallbackType = callbackType
//                }, cancellationToken)).Entity;
//            await _databaseContext.SaveChangesAsync(cancellationToken);

//            return callback;
//        }
//    }
//}
