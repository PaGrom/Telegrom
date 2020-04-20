using System.Collections.Generic;
using System.Threading.Tasks;
using Serilog;
using ShowMustNotGoOn.Core;
using ShowMustNotGoOn.Core.Model;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using User = ShowMustNotGoOn.Core.Model.User;

namespace ShowMustNotGoOn.TelegramService
{
    public class TelegramService : ITelegramService
    {
        private const string NotFoundImage = "https://user-images.githubusercontent.com/24848110/33519396-7e56363c-d79d-11e7-969b-09782f5ccbab.png";

        private readonly ITelegramBotClient _telegramBotClient;
        private readonly ITvShowsService _tvShowsService;
        private readonly ILogger _logger;

        public TelegramService(ITelegramBotClient telegramBotClient,
            ITvShowsService tvShowsService,
            DatabaseContext.DatabaseContext databaseContext,
            ILogger logger)
        {
            _telegramBotClient = telegramBotClient;
            _tvShowsService = tvShowsService;
            _logger = logger;

            var me = _telegramBotClient.GetMeAsync().GetAwaiter().GetResult();
            _logger.Information($"Hello, World! I am user {me.Id} and my name is {me.FirstName}.");
        }

        public async Task SendTextMessageToUserAsync(User user, string text)
        {
            await _telegramBotClient.SendTextMessageAsync(user.TelegramId, text);
        }

        public async Task<BotMessage> SendMessageToUserAsync(User user, BotMessage message)
        {
            var show = await _tvShowsService.GetTvShowAsync(message.CurrentShowId);

            if (string.IsNullOrEmpty(show.Image))
            {
                show.Image = NotFoundImage;
            }

            var buttons = GetButtons(user, message, show);
            var markup = new InlineKeyboardMarkup(buttons);

            var sentMessage = await _telegramBotClient.SendPhotoAsync(user.TelegramId, show.Image,
                $"{show.Title} / {show.TitleOriginal}", replyMarkup: markup);

            message.MessageId = sentMessage.MessageId;

            return message;
        }

        public async Task<BotMessage> UpdateMessageAsync(User user, BotMessage message, string callbackId)
        {
            var show = await _tvShowsService.GetTvShowAsync(message.CurrentShowId);

            if (string.IsNullOrEmpty(show.Image))
            {
                show.Image = NotFoundImage;
            }

            var buttons = GetButtons(user, message, show);
            var markup = new InlineKeyboardMarkup(buttons);

            await _telegramBotClient.AnswerCallbackQueryAsync(callbackId);
            await _telegramBotClient.EditMessageMediaAsync(user.TelegramId, message.MessageId,
                new InputMediaPhoto(new InputMedia(show.Image)));
            var updatedMessage = await _telegramBotClient.EditMessageCaptionAsync(user.TelegramId, message.MessageId,
                $"{show.Title} / {show.TitleOriginal}", markup);

            message.MessageId = updatedMessage.MessageId;

            return message;
        }

        public Task RemoveMessageAsync(User user, BotMessage message)
        {
	        return _telegramBotClient.DeleteMessageAsync(user.TelegramId, message.MessageId);
        }

        private List<List<InlineKeyboardButton>> GetButtons(User user, BotMessage message, TvShow show)
        {
            var buttons = new List<List<InlineKeyboardButton>>
            {
                GetNavigateButtons(message)
            };

            if (user.IsSubscribed(show, SubscriptionType.EndOfShow))
            {
                buttons.Add(new List<InlineKeyboardButton>
                {
                    InlineKeyboardButton.WithCallbackData("Unsubscribe from end of show", "unsubendofshow")
                });
            }
            else
            {
                buttons.Add(new List<InlineKeyboardButton>
                {
                    InlineKeyboardButton.WithCallbackData("Subscribe to end of show", "subendofshow")
                });
            }

            return buttons;
        }

        private List<InlineKeyboardButton> GetNavigateButtons(BotMessage message)
        {
            var buttons = new List<InlineKeyboardButton>();

            if (message.CurrentPage > 0)
            {
                buttons.Add(InlineKeyboardButton.WithCallbackData("Prev", "prev"));
            }

            if (message.CurrentPage < message.TotalPages - 1)
            {
                buttons.Add(InlineKeyboardButton.WithCallbackData("Next", "next"));
            }

            return buttons;
        }
    }
}
