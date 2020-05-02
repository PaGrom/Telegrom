using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
        private readonly DatabaseContext.DatabaseContext _databaseContext;
        private readonly ILogger<TelegramService> _logger;

        public TelegramService(ITelegramBotClient telegramBotClient,
            ITvShowsService tvShowsService,
            DatabaseContext.DatabaseContext databaseContext,
            ILogger<TelegramService> logger)
        {
            _telegramBotClient = telegramBotClient;
            _tvShowsService = tvShowsService;
            _databaseContext = databaseContext;
            _logger = logger;
        }

        public Task<T> MakeRequestAsync<T>(RequestBase<T> request, CancellationToken cancellationToken)
        {
            return _telegramBotClient.MakeRequestAsync(request, cancellationToken);
        }

        public Task SendTextMessageToUserAsync(User user, string text, CancellationToken cancellationToken)
        {
            return MakeRequestAsync(new SendMessageRequest(new ChatId(user.Id), text), cancellationToken);
        }

        public async Task<Message> SendMessageToUserAsync(User user, BotMessage message, CancellationToken cancellationToken)
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

            return sentMessage;
        }

        public async Task<Message> UpdateMessageAsync(User user, BotMessage message, int telegramMessageId, string callbackId, CancellationToken cancellationToken)
        {
            var oldCallbacks = await _databaseContext.Callbacks
                .Where(c => c.BotMessageId == message.Id)
                .ToListAsync(cancellationToken);

            _databaseContext.Callbacks.RemoveRange(oldCallbacks);

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
                telegramMessageId,
                new InputMediaPhoto(new InputMedia(show.Image))), cancellationToken);

            var updatedMessage = await MakeRequestAsync(
                new EditMessageCaptionRequest(
                    new ChatId(user.Id),
                    telegramMessageId,
                    $"{show.Title} / {show.TitleOriginal}")
                {
                    ReplyMarkup = markup
                },
                cancellationToken);

            return updatedMessage;
        }

        public Task RemoveMessageAsync(User user, Message message, CancellationToken cancellationToken)
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
                await GetNavigateButtonsAsync(message, cancellationToken)
            };

            var subscription = await _tvShowsService.GetUserSubscriptionToTvShowAsync(user, show, SubscriptionType.EndOfShow, cancellationToken);

            if (subscription != null)
            {
                var callback = await CreateCallbackAsync(message.Id, CallbackType.UnsubscribeToEndOfShow, cancellationToken);
                buttons.Add(new List<InlineKeyboardButton>
                {
                    InlineKeyboardButton.WithCallbackData("Unsubscribe from end of show", callback.Id.ToString())
                });
            }
            else
            {
                var callback = await CreateCallbackAsync(message.Id, CallbackType.SubscribeToEndOfShow, cancellationToken);
                buttons.Add(new List<InlineKeyboardButton>
                {
                    InlineKeyboardButton.WithCallbackData("Subscribe to end of show", callback.Id.ToString())
                });
            }

            return buttons;
        }

        private async Task<List<InlineKeyboardButton>> GetNavigateButtonsAsync(BotMessage message, CancellationToken cancellationToken)
        {
            var buttons = new List<InlineKeyboardButton>();

            if (message.CurrentPage > 0)
            {
                var callback = await CreateCallbackAsync(message.Id, CallbackType.Prev, cancellationToken);
                buttons.Add(InlineKeyboardButton.WithCallbackData("Prev", callback.Id.ToString()));
            }

            if (message.CurrentPage < message.TotalPages - 1)
            {
                var callback = await CreateCallbackAsync(message.Id, CallbackType.Next, cancellationToken);
                buttons.Add(InlineKeyboardButton.WithCallbackData("Next", callback.Id.ToString()));
            }

            return buttons;
        }

        private async Task<Callback> CreateCallbackAsync(int botMessageId, CallbackType callbackType, CancellationToken cancellationToken)
        {
            var callback = (await _databaseContext.Callbacks
                .AddAsync(new Callback
                {
                    BotMessageId = botMessageId,
                    CallbackType = callbackType
                }, cancellationToken)).Entity;
            await _databaseContext.SaveChangesAsync(cancellationToken);

            return callback;
        }
    }
}
