using System.Linq;
using System.Threading.Tasks;
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
        private readonly IMessage _message;
        private readonly ITelegramService _telegramService;
        private readonly IUsersService _usersService;
        private readonly ITvShowsService _tvShowsService;
        private readonly DatabaseContext.DatabaseContext _databaseContext;
        private readonly ILogger _logger;

        public MessageHandler(IMessage message,
            ITelegramService telegramService,
            IUsersService usersService,
            ITvShowsService tvShowsService,
            DatabaseContext.DatabaseContext databaseContext,
            ILogger logger)
        {
            _message = message;
            _telegramService = telegramService;
            _usersService = usersService;
            _tvShowsService = tvShowsService;
            _databaseContext = databaseContext;
            _logger = logger;
        }

        public async Task HandleAsync()
        {
            await using var transaction = await _databaseContext.Database.BeginTransactionAsync();
            
            switch (_message)
            {
                case TelegramMessageReceivedEvent e:
                    await HandleMessageAsync(e.UserMessage);
                    break;
                case TelegramCallbackButtonReceivedEvent e:
                    await HandleCallbackButtonAsync(e.CallbackButton);
                    break;
            }

            await transaction.CommitAsync();
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