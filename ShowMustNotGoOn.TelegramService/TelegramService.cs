using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Newtonsoft.Json;
using Serilog;
using ShowMustNotGoOn.Core;
using ShowMustNotGoOn.Core.Model;
using ShowMustNotGoOn.Core.Model.CallbackQuery;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using CallbackQuery = ShowMustNotGoOn.Core.Model.CallbackQuery.CallbackQuery;
using Message = ShowMustNotGoOn.Core.Model.Message;
using User = ShowMustNotGoOn.Core.Model.User;

namespace ShowMustNotGoOn.TelegramService
{
    public class TelegramService : ITelegramService, IDisposable
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private Action<Message> _messageReceivedHandler;
        private Action<CallbackQuery> _callbackQueryReceivedHandler;

        public TelegramService(ITelegramBotClient telegramBotClient,
            IMapper mapper,
            ILogger logger)
        {
            _telegramBotClient = telegramBotClient;
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

        private void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs e)
        {
            var callback = e.CallbackQuery;
            _logger.Information("Get callback query {@callback}", callback);
            _callbackQueryReceivedHandler?.Invoke(_mapper.Map<CallbackQuery>(callback));
        }

        public void SetMessageReceivedHandler(Action<Message> handler)
        {
            _messageReceivedHandler = handler;
        }

        public void SetCallbackQueryReceivedHandler(Action<CallbackQuery> handler)
        {
            _callbackQueryReceivedHandler = handler;
        }

        private void BotOnMessageReceived(object sender, MessageEventArgs e)
        {
            var message = e.Message;
            _logger.Information("Get message {@message}", message);
            _messageReceivedHandler?.Invoke(_mapper.Map<Message>(message));
        }

        public async Task SendTextMessageToUser(User user, string text)
        {
            await _telegramBotClient.SendTextMessageAsync(user.TelegramId, text);
        }

        public async Task SendTvShowToUser(User user, TvShow show,
            int? nextNavigateCallbackQueryDataId)
        {
            if (nextNavigateCallbackQueryDataId != null)
            {
                var button = InlineKeyboardButton.WithCallbackData("Next",
                    nextNavigateCallbackQueryDataId.ToString());
                var markup = new InlineKeyboardMarkup(button);
                await _telegramBotClient.SendPhotoAsync(user.TelegramId, show.Image,
                    $"{show.Title} / {show.TitleOriginal}", replyMarkup: markup);
                return;
            }

            await _telegramBotClient.SendPhotoAsync(user.TelegramId, show.Image,
                $"{show.Title} / {show.TitleOriginal}");
        }

        public async Task UpdateTvShowMessage(User user, TvShow show, int messageId,
            int? prevNavigateCallbackQueryDataId,
            int? nextNavigateCallbackQueryDataId)
        {
            var navigateButtons = new List<InlineKeyboardButton>();
            if (prevNavigateCallbackQueryDataId != null)
            {
                navigateButtons.Add(InlineKeyboardButton.WithCallbackData("Prev",
                    prevNavigateCallbackQueryDataId.ToString()));
            }
            if (nextNavigateCallbackQueryDataId != null)
            {
                navigateButtons.Add(InlineKeyboardButton.WithCallbackData("Next",
                    nextNavigateCallbackQueryDataId.ToString()));
            }
            var markup = new InlineKeyboardMarkup(navigateButtons);

            await _telegramBotClient.EditMessageMediaAsync(user.TelegramId, messageId,
                new InputMediaPhoto(new InputMedia(show.Image)));
            await _telegramBotClient.EditMessageCaptionAsync(user.TelegramId, messageId,
                $"{show.Title} / {show.TitleOriginal}", markup);
        }

        public void Dispose()
        {
            _telegramBotClient.StopReceiving();
        }
    }
}
