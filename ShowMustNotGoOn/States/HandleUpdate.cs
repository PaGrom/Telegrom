using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShowMustNotGoOn.Core;
using ShowMustNotGoOn.Core.TelegramModel;
using ShowMustNotGoOn.DatabaseContext.Extensions;
using ShowMustNotGoOn.DatabaseContext.Model;
using ShowMustNotGoOn.StateMachine;

namespace ShowMustNotGoOn.States
{
    internal sealed class HandleUpdate : ChooseHandleState
    {
        public HandleUpdate(IStateContext stateContext)
            : base(stateContext,
                new (Func<bool>, Type)[]
                {
                    (() => stateContext.UpdateContext.Update is Message, typeof(HandleMessage)),
                    (() => stateContext.UpdateContext.Update is CallbackQuery, typeof(Finish)),
                })
        {
        }
    }

    internal abstract class ChooseHandleState : IState
    {
        private readonly IStateContext _stateContext;
        private readonly IEnumerable<(Func<bool> condition, Type type)> _conditions;

        public ChooseHandleState(IStateContext stateContext, IEnumerable<(Func<bool> condition, Type type)> conditions)
        {
            _stateContext = stateContext;
            _conditions = conditions;
        }

        public Task Handle(CancellationToken cancellationToken)
        {
            foreach (var (condition, type) in _conditions)
            {
                if (condition.Invoke())
                {
                    //_stateContext.StateMachineContext.MoveTo(type);
                    break;
                }
            }

            return Task.CompletedTask;
        }
    }

    internal abstract class ChooseState : IState
    {
        private readonly IStateContext _stateContext;
        private readonly IEnumerable<(Func<CancellationToken, Task<bool>> condition, Type type)> _conditions;

        public ChooseState(IStateContext stateContext, IEnumerable<(Func<CancellationToken, Task<bool>> condition, Type type)> conditions)
        {
            _stateContext = stateContext;
            _conditions = conditions;
        }

        public async Task OnEnter(CancellationToken cancellationToken)
        {
            foreach (var (condition, type) in _conditions)
            {
                if (await condition.Invoke(cancellationToken))
                {
                    //_stateContext.StateMachineContext.MoveTo(type);
                    break;
                }
            }
        }
    }

    public abstract class DoActionAndThen<TNext> : IState
        where TNext : IState
    {
        private readonly IStateContext _stateContext;
        private readonly Func<CancellationToken, Task> _action;

        protected DoActionAndThen(IStateContext stateContext, Func<CancellationToken, Task> action)
        {
            _stateContext = stateContext;
            _action = action;
        }

        public async Task OnEnter(CancellationToken cancellationToken)
        {
            await _action(cancellationToken);
            //_stateContext.StateMachineContext.MoveTo<TNext>();
        }
    }

    internal sealed class HandleMessage : ChooseState
    {
        public HandleMessage(IStateContext stateContext)
            : base(stateContext,
                new (Func<CancellationToken, Task<bool>>, Type)[]
                {
                    (ct => Task.FromResult(((Message)stateContext.UpdateContext.Update).Text.Trim().StartsWith("/")), typeof(Finish)),
                    (ct => Task.FromResult(true), typeof(HandleTextMessage)),
                })
        {
        }
    }

    internal sealed class SendCantFindTvShowMessage : SendMessage
    {
        public SendCantFindTvShowMessage(IStateContext stateContext) : base(stateContext, "Can't find tv show for you")
        {
        }
    }

    internal sealed class HandleTextMessage : ChooseState
    {
        public HandleTextMessage(IStateContext stateContext, ITvShowsService tvShowsService)
            : base(stateContext,
                new (Func<CancellationToken, Task<bool>>, Type)[]
                {
                    (async ct => !(await tvShowsService.SearchTvShowsAsync(((Message)stateContext.UpdateContext.Update).Text.Trim(), ct)).Any(), typeof(SendCantFindTvShowMessage)),
                    (ct => Task.FromResult(true), typeof(FindTvShowAndGenerateMessage)),
                })
        {
        }
    }

    internal sealed class FindTvShowAndGenerateMessage : DoActionAndThen<GenerateSendPhotoRequest>
    {
        public FindTvShowAndGenerateMessage(IStateContext stateContext,
            ITvShowsService tvShowsService,
            DatabaseContext.DatabaseContext databaseContext) : base(stateContext,
            async ct =>
            {
                var messageTextString = ((Message)stateContext.UpdateContext.Update).Text.Trim();

                const int pageCount = 0;

                var tvShows = (await tvShowsService.SearchTvShowsAsync(messageTextString, ct)).ToList();

                var messageText = await databaseContext.MessageTexts
                    .AddIfNotExistsAsync(new MessageText
                    {
                        Text = messageTextString
                    }, s => s.Text == messageTextString, ct);

                await databaseContext.SaveChangesAsync(ct);

                var botMessage = new BotMessage
                {
                    UserId = stateContext.UpdateContext.SessionContext.User.Id,
                    BotCommandType = null,
                    MessageTextId = messageText.Id,
                    MyShowsId = tvShows.First().Id,
                    CurrentPage = pageCount,
                    TotalPages = tvShows.Count
                };

                await databaseContext.BotMessages.AddAsync(botMessage, ct);
                await databaseContext.SaveChangesAsync(ct);

                stateContext.Objects[typeof(BotMessage)] = botMessage;
            })
        {
        }
    }

    internal sealed class GenerateSendPhotoRequest : DoActionAndThen<GenerateKeyboardMarkup>
    {
        public GenerateSendPhotoRequest(IStateContext stateContext, ITvShowsService tvShowsService) : base(stateContext,
            async ct =>
            {
                const string notFoundImage = "https://images-na.ssl-images-amazon.com/images/I/312yeogBelL._SX466_.jpg";

                var message = (BotMessage)stateContext.Objects[typeof(BotMessage)];

                var show = await tvShowsService.GetTvShowByMyShowsIdAsync(message.MyShowsId, ct)
                           ?? await tvShowsService.GetTvShowFromMyShowsAsync(message.MyShowsId, ct);

                if (string.IsNullOrEmpty(show.Image))
                {
                    show.Image = notFoundImage;
                }

                var keyboardMarkup = InlineKeyboardMarkup.Empty();

                var request = new SendPhotoRequest(stateContext.UpdateContext.SessionContext.User.Id, show.Image)
                {
                    Caption = $"{show.Title} / {show.TitleOriginal}",
                    ReplyMarkup = keyboardMarkup
                };

                stateContext.Objects[typeof(SendPhotoRequest)] = request;
                stateContext.Objects[typeof(InlineKeyboardMarkup)] = keyboardMarkup;
                stateContext.Objects[typeof(TvShow)] = show;
            })
        {
        }
    }

    internal sealed class GenerateKeyboardMarkup : DoActionAndThen<GenerateNavigationButtons>
    {
        public GenerateKeyboardMarkup(IStateContext stateContext) : base(stateContext,
            ct => Task.CompletedTask)
        {
        }
    }

    internal sealed class GenerateNavigationButtons : DoActionAndThen<GenerateSubscriptionsButtons>
    {
        public GenerateNavigationButtons(IStateContext stateContext, DatabaseContext.DatabaseContext databaseContext) : base(stateContext,
            async ct =>
            {
                var message = (BotMessage)stateContext.Objects[typeof(BotMessage)];

                var keyboardMarkup = (InlineKeyboardMarkup)stateContext.Objects[typeof(InlineKeyboardMarkup)];

                var buttons = new List<InlineKeyboardButton>();

                if (message.CurrentPage > 0)
                {
                    var callback = await CreateCallbackAsync(message.Id, CallbackType.Prev, ct);
                    buttons.Add(InlineKeyboardButton.WithCallbackData("Prev", callback.Id.ToString()));
                }

                if (message.CurrentPage < message.TotalPages - 1)
                {
                    var callback = await CreateCallbackAsync(message.Id, CallbackType.Next, ct);
                    buttons.Add(InlineKeyboardButton.WithCallbackData("Next", callback.Id.ToString()));
                }

                keyboardMarkup.AddRow(buttons);

                async Task<Callback> CreateCallbackAsync(int botMessageId, CallbackType callbackType, CancellationToken cancellationToken)
                {
                    var callback = (await databaseContext.Callbacks
                        .AddAsync(new Callback
                        {
                            BotMessageId = botMessageId,
                            CallbackType = callbackType
                        }, cancellationToken)).Entity;
                    await databaseContext.SaveChangesAsync(cancellationToken);

                    return callback;
                }
            })
        {
        }
    }

    internal sealed class GenerateSubscriptionsButtons : DoActionAndThen<SendSendPhotoRequest>
    {
        public GenerateSubscriptionsButtons(IStateContext stateContext, DatabaseContext.DatabaseContext databaseContext, ITvShowsService tvShowsService) : base(stateContext,
            async ct =>
            {
                var message = (BotMessage)stateContext.Objects[typeof(BotMessage)];

                var keyboardMarkup = (InlineKeyboardMarkup)stateContext.Objects[typeof(InlineKeyboardMarkup)];

                var show = (TvShow)stateContext.Objects[typeof(TvShow)];

                var buttons = new List<InlineKeyboardButton>();

                var subscription = await tvShowsService.GetUserSubscriptionToTvShowAsync(stateContext.UpdateContext.SessionContext.User, show, SubscriptionType.EndOfShow, ct);

                if (subscription != null)
                {
                    var callback = await CreateCallbackAsync(message.Id, CallbackType.UnsubscribeToEndOfShow, ct);
                    buttons.Add(InlineKeyboardButton.WithCallbackData("Unsubscribe from end of show", callback.Id.ToString()));
                }
                else
                {
                    var callback = await CreateCallbackAsync(message.Id, CallbackType.SubscribeToEndOfShow, ct);
                    buttons.Add(InlineKeyboardButton.WithCallbackData("Subscribe to end of show", callback.Id.ToString()));
                }

                keyboardMarkup.AddRow(buttons);

                async Task<Callback> CreateCallbackAsync(int botMessageId, CallbackType callbackType, CancellationToken cancellationToken)
                {
                    var callback = (await databaseContext.Callbacks
                        .AddAsync(new Callback
                        {
                            BotMessageId = botMessageId,
                            CallbackType = callbackType
                        }, cancellationToken)).Entity;
                    await databaseContext.SaveChangesAsync(cancellationToken);

                    return callback;
                }
            })
        {
        }
    }

    internal sealed class SendSendPhotoRequest : DoActionAndThen<HandleUpdate>
    {
        public SendSendPhotoRequest(IStateContext stateContext) : base(stateContext,
            async ct =>
            {
                var request = (SendPhotoRequest)stateContext.Objects[typeof(SendPhotoRequest)];

                await stateContext.UpdateContext.SessionContext.PostRequestAsync(request, ct);
            })
        {
        }
    }

    internal sealed class Finish : IState {}
}
