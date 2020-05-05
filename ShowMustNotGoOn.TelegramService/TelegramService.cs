using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShowMustNotGoOn.Core;
using ShowMustNotGoOn.Core.Request;
using ShowMustNotGoOn.Core.TelegramModel;
using ShowMustNotGoOn.DatabaseContext.Model;
using Telegram.Bot;

namespace ShowMustNotGoOn.TelegramService
{
    public class TelegramService : ITelegramService
    {
        private const string NotFoundImage = "https://images-na.ssl-images-amazon.com/images/I/312yeogBelL._SX466_.jpg";

        private readonly RequestContext _requestContext;
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly ITvShowsService _tvShowsService;
        private readonly DatabaseContext.DatabaseContext _databaseContext;
        private readonly IMapper _mapper;
        private readonly ILogger<TelegramService> _logger;

        public TelegramService(RequestContext requestContext,
            ITelegramBotClient telegramBotClient,
            ITvShowsService tvShowsService,
            DatabaseContext.DatabaseContext databaseContext,
            IMapper mapper,
            ILogger<TelegramService> logger)
        {
            _requestContext = requestContext;
            _telegramBotClient = telegramBotClient;
            _tvShowsService = tvShowsService;
            _databaseContext = databaseContext;
            _mapper = mapper;
            _logger = logger;
        }

        public Task MakeRequestAsync<T>(Telegram.Bot.Requests.RequestBase<T> request, CancellationToken cancellationToken)
        {
            return _telegramBotClient.MakeRequestAsync(request, cancellationToken);
        }

        public Task SendTextMessageToUserAsync(User user, string text, CancellationToken cancellationToken)
        {
            var request = new SendMessageRequest(user.Id, text);
            return MakeRequestAsync(_mapper.Map<Telegram.Bot.Requests.SendMessageRequest>(request), cancellationToken);
        }

        public async Task SendMessageToUserAsync(User user, BotMessage message, CancellationToken cancellationToken)
        {
            var show = await _tvShowsService.GetTvShowByMyShowsIdAsync(message.MyShowsId, cancellationToken)
                       ?? await _tvShowsService.GetTvShowFromMyShowsAsync(message.MyShowsId, cancellationToken);

            if (string.IsNullOrEmpty(show.Image))
            {
                show.Image = NotFoundImage;
            }

            var buttons = await GetButtonsAsync(user, message, show, cancellationToken);
            var markup = new InlineKeyboardMarkup(buttons);

            var request = new SendPhotoRequest(user.Id, show.Image)
            {
                Caption = $"{show.Title} / {show.TitleOriginal}",
                ReplyMarkup = markup
            };

            await MakeRequestAsync(
                _mapper.Map<Telegram.Bot.Requests.SendPhotoRequest>(request),
                cancellationToken);
        }

        public async Task UpdateMessageAsync(User user, BotMessage message, int telegramMessageId, string callbackId, CancellationToken cancellationToken)
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

            await MakeRequestAsync(
                new EditMessageCaptionRequest(
                    new ChatId(user.Id),
                    telegramMessageId,
                    $"{show.Title} / {show.TitleOriginal}")
                {
                    ReplyMarkup = markup
                },
                cancellationToken);
        }

        public Task RemoveMessageAsync(User user, int messageId, CancellationToken cancellationToken)
        {
            return MakeRequestAsync(new DeleteMessageRequest(
                    new ChatId(user.Id),
                    messageId),
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
