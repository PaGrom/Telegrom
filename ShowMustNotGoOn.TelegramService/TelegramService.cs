using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ShowMustNotGoOn.Core;
using ShowMustNotGoOn.DatabaseContext.Model;
using Telegram.Bot;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace ShowMustNotGoOn.TelegramService
{
    public class TelegramService : ITelegramService
    {
        private const string NotFoundImage = "https://images-na.ssl-images-amazon.com/images/I/312yeogBelL._SX466_.jpg";

        private readonly ITelegramBotClient _telegramBotClient;
        private readonly ITvShowsService _tvShowsService;
        private readonly ILogger<TelegramService> _logger;

        public TelegramService(ITelegramBotClient telegramBotClient,
            ITvShowsService tvShowsService,
            ILogger<TelegramService> logger)
        {
            _telegramBotClient = telegramBotClient;
            _tvShowsService = tvShowsService;
            _logger = logger;

            var me = _telegramBotClient.GetMeAsync().GetAwaiter().GetResult();
            _logger.LogInformation($"Hello, World! I am identityUser {me.Id} and my name is {me.FirstName}.");
        }

        public Task<T> MakeRequestAsync<T>(RequestBase<T> request, CancellationToken cancellationToken)
        {
            return _telegramBotClient.MakeRequestAsync(request, cancellationToken);
        }

        public Task SendTextMessageToUserAsync(User user, string text, CancellationToken cancellationToken)
        {
            return MakeRequestAsync(new SendMessageRequest(new ChatId(user.Id), text), cancellationToken);
        }

        public async Task<BotMessage> SendMessageToUserAsync(User user, BotMessage message, CancellationToken cancellationToken)
        {
            var show = await _tvShowsService.GetTvShowByMyShowsIdAsync(message.MyShowsId, cancellationToken)
                       ?? await _tvShowsService.GetTvShowFromMyShowsAsync(message.MyShowsId, cancellationToken);

            if (string.IsNullOrEmpty(show.Image))
            {
                show.Image = NotFoundImage;
            }

            var buttons = await GetButtonsAsync(user, message, show, cancellationToken);
            var markup = new InlineKeyboardMarkup(buttons);

            var sentMessage = await MakeRequestAsync(
                new SendPhotoRequest(new ChatId(user.Id), new InputOnlineFile(show.Image))
                {
                    Caption = $"{show.Title} / {show.TitleOriginal}",
                    ReplyMarkup = markup
                },
                cancellationToken);

            message.MessageId = sentMessage.MessageId;

            return message;
        }

        public async Task<BotMessage> UpdateMessageAsync(User user, BotMessage message, string callbackId, CancellationToken cancellationToken)
        {
            var show = await _tvShowsService.GetTvShowByMyShowsIdAsync(message.MyShowsId, cancellationToken)
                       ?? await _tvShowsService.GetTvShowFromMyShowsAsync(message.MyShowsId, cancellationToken);

            if (string.IsNullOrEmpty(show.Image))
            {
                show.Image = NotFoundImage;
            }

            var buttons = await GetButtonsAsync(user, message, show, cancellationToken);
            var markup = new InlineKeyboardMarkup(buttons);

            await MakeRequestAsync(new AnswerCallbackQueryRequest(callbackId), cancellationToken);

            await MakeRequestAsync(new EditMessageMediaRequest(
                new ChatId(user.Id),
                message.MessageId,
                new InputMediaPhoto(new InputMedia(show.Image))), cancellationToken);

            var updatedMessage = await MakeRequestAsync(
                new EditMessageCaptionRequest(
                    new ChatId(user.Id),
                    message.MessageId,
                    $"{show.Title} / {show.TitleOriginal}")
                {
                    ReplyMarkup = markup
                },
                cancellationToken);

            message.MessageId = updatedMessage.MessageId;

            return message;
        }

        public Task RemoveMessageAsync(User user, BotMessage message, CancellationToken cancellationToken)
        {
            return MakeRequestAsync(new DeleteMessageRequest(
                    new ChatId(user.Id),
                    message.MessageId),
                cancellationToken);
        }

        private async Task<List<List<InlineKeyboardButton>>> GetButtonsAsync(User user, BotMessage message, TvShow show, CancellationToken cancellationToken)
        {
            var buttons = new List<List<InlineKeyboardButton>>
            {
                GetNavigateButtons(message)
            };

            var subscription = await _tvShowsService.GetUserSubscriptionToTvShowAsync(user, show, SubscriptionType.EndOfShow, cancellationToken);

            if (subscription != null)
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
