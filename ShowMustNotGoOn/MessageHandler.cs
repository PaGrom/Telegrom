using System;
using System.Linq;
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
    public sealed class MessageHandler
    {
        private readonly ITelegramService _telegramService;
        private readonly IUsersService _usersService;
        private readonly ITvShowsService _tvShowsService;
        private readonly DatabaseContext.DatabaseContext _databaseContext;
        private readonly ILogger _logger;

        public MessageHandler(ITelegramService telegramService,
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
        }

        public async Task HandleAsync(User user, IMessage message)
        {
            try
            {
                await using var transaction = await _databaseContext.Database.BeginTransactionAsync();

                switch (message)
                {
                    case TelegramMessageReceivedEvent e:
                        await HandleMessageAsync(e.UserMessage);
                        break;
                    case TelegramCallbackButtonReceivedEvent e:
                        await HandleCallbackButtonAsync(user, e.UserCallback);
                        break;
                }

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error during message handling");
            }
        }

        private async Task HandleCallbackButtonAsync(User user, UserCallback userCallback)
        {
            var botMessage = await _databaseContext.BotMessages
                .SingleAsync(m => user.TelegramId == userCallback.User.TelegramId
                                  && m.MessageId == userCallback.MessageId);

            var callbackButton = new CallbackButton
            {
                Message = botMessage,
                CallbackId = userCallback.CallbackId,
                CallbackData = userCallback.CallbackData
            };

            if (botMessage.BotCommandType == BotCommandType.Subscriptions)
            {
                await HandleSubscriptionsCallbackButtonAsync(user, callbackButton);
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
                    await _usersService.SubscribeUserToTvShowAsync(user,
                        tvShows[botMessage.CurrentPage],
                        SubscriptionType.EndOfShow);
                    break;
                case "unsubendofshow":
                    await _usersService.UnsubscribeUserFromTvShowAsync(user,
                        tvShows[botMessage.CurrentPage],
                        SubscriptionType.EndOfShow);
                    break;
                default:
                    return;
            }

            botMessage.CurrentShowId = tvShows[botMessage.CurrentPage].MyShowsId;

            botMessage = await _telegramService.UpdateMessageAsync(user, botMessage, callbackButton.CallbackId);

            _databaseContext.BotMessages.Update(botMessage);
            await _databaseContext.SaveChangesAsync();
        }

        private async Task HandleSubscriptionsCallbackButtonAsync(User user, CallbackButton callbackButton)
        {
            var botMessage = callbackButton.Message;

            var tvShows = user.Subscriptions
                .Select(s => s.TvShow)
                .ToList();

            // handle navigate buttons
            if (callbackButton.CallbackData == "next"
                || callbackButton.CallbackData == "prev")
            {
                if (!tvShows.Any())
                {
                    await _telegramService.RemoveMessageAsync(user, botMessage);
                    _databaseContext.BotMessages.Remove(botMessage);
                    await _databaseContext.SaveChangesAsync();
                    await _telegramService.SendTextMessageToUserAsync(user,
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

                switch (callbackButton.CallbackData)
                {
	                case "next":
	                {
		                if (currentPage < tvShows.Count - 1)
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

                botMessage.CurrentShowId = tvShows[botMessage.CurrentPage].MyShowsId;
            }

            switch (callbackButton.CallbackData)
            {
	            // handle subscribe button
	            case "subendofshow":
	            {
		            var tvShow = await _tvShowsService.GetTvShowAsync(botMessage.CurrentShowId);
		            if (!user.IsSubscribed(tvShow, SubscriptionType.EndOfShow))
		            {
			            await _usersService.SubscribeUserToTvShowAsync(user,
				            tvShow,
				            SubscriptionType.EndOfShow);
		            }

		            break;
	            }
	            // handle unsubscribe button
	            case "unsubendofshow":
	            {
		            var tvShow = await _tvShowsService.GetTvShowAsync(botMessage.CurrentShowId);
		            if (user.IsSubscribed(tvShow, SubscriptionType.EndOfShow))
		            {
			            await _usersService.UnsubscribeUserFromTvShowAsync(user,
				            tvShow,
				            SubscriptionType.EndOfShow);
		            }

		            break;
	            }
            }

            botMessage.TotalPages = tvShows.Count;

            botMessage = await _telegramService.UpdateMessageAsync(user, botMessage, callbackButton.CallbackId);

            _databaseContext.BotMessages.Update(botMessage);
            await _databaseContext.SaveChangesAsync();
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
                UserId = userMessage.User.Id,
                BotCommandType = null,
                SearchPattern = searchPattern,
                CurrentShowId = tvShows.First().MyShowsId,
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
                        UserId = userMessage.User.Id,
                        BotCommandType = BotCommandType.Subscriptions,
                        SearchPattern = null,
                        CurrentShowId = tvShows.First().MyShowsId,
                        CurrentPage = pageCount,
                        TotalPages = tvShows.Count
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