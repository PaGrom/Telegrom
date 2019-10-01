using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Serilog;
using ShowMustNotGoOn.Core;
using ShowMustNotGoOn.Core.Model;
using ShowMustNotGoOn.Core.Model.Callback;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using CallbackQuery = ShowMustNotGoOn.Core.Model.Callback.CallbackQuery;
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

        public async Task<Message> SendTvShowToUserAsync(User user, TvShow show,
            ButtonCallbackQueryData nextNavigateCallbackQueryData,
            ButtonCallbackQueryData subscribeEndOfShowCallbackQueryData)
        {
            var buttons = new List<List<InlineKeyboardButton>>();

            if (nextNavigateCallbackQueryData?.Id != null)
            {
                var button = InlineKeyboardButton.WithCallbackData("Next",
                    nextNavigateCallbackQueryData.Id.ToString());
                
                buttons.Add(new List<InlineKeyboardButton> { button });
            }

            var subscribeEndOfShowButtonText = subscribeEndOfShowCallbackQueryData.CallbackQueryType switch
            {
                CallbackQueryType.SubscribeEndOfShow => "Subscribe to end of show",
                CallbackQueryType.UnsubscribeEndOfShow => "Unsubscribe to end of show",
                _ => string.Empty
            };

            buttons.Add(new List<InlineKeyboardButton>
            {
                InlineKeyboardButton.WithCallbackData(subscribeEndOfShowButtonText,
                    subscribeEndOfShowCallbackQueryData.Id.ToString())
            });

            var markup = new InlineKeyboardMarkup(buttons);
            var sentMessage = await _telegramBotClient.SendPhotoAsync(user.TelegramId, show.Image,
                $"{show.Title} / {show.TitleOriginal}", replyMarkup: markup);

            return _mapper.Map<Message>(sentMessage);
        }

        public async Task<Message> UpdateTvShowMessageAsync(User user, TvShow show,
            CallbackQuery callbackQuery,
            ButtonCallbackQueryData prevNavigateCallbackQueryData,
            ButtonCallbackQueryData nextNavigateCallbackQueryData,
            ButtonCallbackQueryData subscribeEndOfShowCallbackQueryData)
        {
            var navigateButtons = new List<InlineKeyboardButton>();
            var buttons = new List<List<InlineKeyboardButton>> { navigateButtons };
            if (prevNavigateCallbackQueryData?.Id != null)
            {
                navigateButtons.Add(InlineKeyboardButton.WithCallbackData("Prev",
                    prevNavigateCallbackQueryData.Id.ToString()));
            }
            if (nextNavigateCallbackQueryData?.Id != null)
            {
                navigateButtons.Add(InlineKeyboardButton.WithCallbackData("Next",
                    nextNavigateCallbackQueryData.Id.ToString()));
            }

            var subscribeEndOfShowButtonText = subscribeEndOfShowCallbackQueryData.CallbackQueryType switch
            {
                CallbackQueryType.SubscribeEndOfShow => "Subscribe to end of show",
                CallbackQueryType.UnsubscribeEndOfShow => "Unsubscribe to end of show",
                _ => string.Empty
            };

            buttons.Add(new List<InlineKeyboardButton>
            {
                InlineKeyboardButton.WithCallbackData(subscribeEndOfShowButtonText,
                    subscribeEndOfShowCallbackQueryData.Id.ToString())
            });

            var markup = new InlineKeyboardMarkup(buttons);

            if (string.IsNullOrEmpty(show.Image))
            {
                show.Image = "https://user-images.githubusercontent.com/24848110/33519396-7e56363c-d79d-11e7-969b-09782f5ccbab.png";
            }

            await _telegramBotClient.AnswerCallbackQueryAsync(callbackQuery.Id);
            await _telegramBotClient.EditMessageMediaAsync(user.TelegramId, callbackQuery.Message.MessageId,
                new InputMediaPhoto(new InputMedia(show.Image)));
            var updatedMessage = await _telegramBotClient.EditMessageCaptionAsync(user.TelegramId, callbackQuery.Message.MessageId,
                $"{show.Title} / {show.TitleOriginal}", markup);

            return _mapper.Map<Message>(updatedMessage);
        }

        public void Dispose()
        {
            _telegramBotClient.StopReceiving();
        }
    }
}
