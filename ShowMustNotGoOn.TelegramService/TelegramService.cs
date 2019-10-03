using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Serilog;
using ShowMustNotGoOn.Core;
using ShowMustNotGoOn.Core.Model;
using ShowMustNotGoOn.Core.Model.Callback;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using User = ShowMustNotGoOn.Core.Model.User;

namespace ShowMustNotGoOn.TelegramService
{
    public class TelegramService : ITelegramService, IDisposable
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly IUsersService _usersService;
        private readonly ITvShowsService _tvShowsService;
        private readonly DatabaseContext.DatabaseContext _databaseContext;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private Action<UserMessage> _messageReceivedHandler;
        private Action<CallbackButton> _callbackButtonReceivedHandler;

        public TelegramService(ITelegramBotClient telegramBotClient,
            IUsersService usersService,
            ITvShowsService tvShowsService,
            DatabaseContext.DatabaseContext databaseContext,
            IMapper mapper,
            ILogger logger)
        {
            _telegramBotClient = telegramBotClient;
            _usersService = usersService;
            _tvShowsService = tvShowsService;
            _databaseContext = databaseContext;
            _mapper = mapper;
            _logger = logger;

            var me = _telegramBotClient.GetMeAsync().GetAwaiter().GetResult();
            _logger.Information($"Hello, World! I am user {me.Id} and my name is {me.FirstName}.");
        }

        public void Start()
        {
            RegisterHandlers();
            _telegramBotClient.StartReceiving();
        }

        private void RegisterHandlers()
        {
            _telegramBotClient.OnMessage += BotOnMessageReceived;
            _telegramBotClient.OnCallbackQuery += BotOnCallbackQueryReceived;
        }

        private async void BotOnMessageReceived(object sender, MessageEventArgs e)
        {
            var telegramMessage = e.Message;
            _logger.Information("Get message {@message}", telegramMessage);
            var message = _mapper.Map<UserMessage>(telegramMessage);

            message.User = await _usersService.AddOrUpdateUserAsync(message.User);

            _messageReceivedHandler?.Invoke(message);
        }

        private async void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs e)
        {
            var callback = e.CallbackQuery;
            _logger.Information("Get callback query {@callback}", callback);
            var botMessage = await _databaseContext.BotMessages
                .Include(m => m.User)
                .SingleAsync(m => m.User.TelegramId == callback.From.Id
                             && m.MessageId == callback.Message.MessageId);
            var callbackButton = new CallbackButton
            {
                Message = botMessage,
                CallbackId = callback.Id,
                CallbackData = callback.Data
            };
            _callbackButtonReceivedHandler?.Invoke(callbackButton);
        }

        public void SetMessageReceivedHandler(Action<UserMessage> handler)
        {
            _messageReceivedHandler = handler;
        }

        public void SetCallbackButtonReceivedHandler(Action<CallbackButton> handler)
        {
            _callbackButtonReceivedHandler = handler;
        }

        public async Task SendTextMessageToUserAsync(User user, string text)
        {
            await _telegramBotClient.SendTextMessageAsync(user.TelegramId, text);
        }

        public async Task SendMessageToUserAsync(User user, BotMessage message)
        {
            var buttons = GetButtons(message);
            var markup = new InlineKeyboardMarkup(buttons);
            var show = await _tvShowsService.GetTvShowAsync(message.CurrentShowId);

            var sentMessage = await _telegramBotClient.SendPhotoAsync(user.TelegramId, show.Image,
                $"{show.Title} / {show.TitleOriginal}", replyMarkup: markup);

            message.MessageId = sentMessage.MessageId;

            await using var transaction = await _databaseContext.Database.BeginTransactionAsync();
            _databaseContext.BotMessages.Add(message);
            await _databaseContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }

        public async Task UpdateMessageAsync(BotMessage message, string callbackId)
        {
            var user = message.User;
            var buttons = GetButtons(message);
            var markup = new InlineKeyboardMarkup(buttons);
            var show = await _tvShowsService.GetTvShowAsync(message.CurrentShowId);

            await _telegramBotClient.AnswerCallbackQueryAsync(callbackId);
            await _telegramBotClient.EditMessageMediaAsync(user.TelegramId, message.MessageId,
                new InputMediaPhoto(new InputMedia(show.Image)));
            var updatedMessage = await _telegramBotClient.EditMessageCaptionAsync(user.TelegramId, message.MessageId,
                $"{show.Title} / {show.TitleOriginal}", markup);

            await using var transaction = await _databaseContext.Database.BeginTransactionAsync();
            _databaseContext.BotMessages.Update(message);
            await _databaseContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }

        private List<List<InlineKeyboardButton>> GetButtons(BotMessage message)
        {
            var buttons = new List<List<InlineKeyboardButton>>
            {
                GetNavigateButtons(message),
            };

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

        public void Dispose()
        {
            _telegramBotClient.StopReceiving();
        }
    }
}
