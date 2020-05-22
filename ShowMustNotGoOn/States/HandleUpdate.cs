using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ShowMustNotGoOn.DatabaseContext.Extensions;
using ShowMustNotGoOn.StateMachine;

namespace ShowMustNotGoOn.States
{
    internal sealed class HandleUpdate : StateBase
    {
    }

    //internal abstract class ChooseState : StateBase
    //{
    //    private readonly IStateContext _stateContext;
    //    private readonly IEnumerable<(Func<CancellationToken, Task<bool>> condition, Type type)> _conditions;

    //    public ChooseState(IStateContext stateContext, IEnumerable<(Func<CancellationToken, Task<bool>> condition, Type type)> conditions)
    //    {
    //        _stateContext = stateContext;
    //        _conditions = conditions;
    //    }

    //    public async Task OnEnter(CancellationToken cancellationToken)
    //    {
    //        foreach (var (condition, type) in _conditions)
    //        {
    //            if (await condition.Invoke(cancellationToken))
    //            {
    //                //_stateContext.StateMachineContext.MoveTo(type);
    //                break;
    //            }
    //        }
    //    }
    //}

    //public abstract class DoActionAndThen<TNext> : StateBase
    //    where TNext : StateBase
    //{
    //    private readonly IStateContext _stateContext;
    //    private readonly Func<CancellationToken, Task> _action;

    //    protected DoActionAndThen(IStateContext stateContext, Func<CancellationToken, Task> action)
    //    {
    //        _stateContext = stateContext;
    //        _action = action;
    //    }

    //    public async Task OnEnter(CancellationToken cancellationToken)
    //    {
    //        await _action(cancellationToken);
    //        //_stateContext.StateMachineContext.MoveTo<TNext>();
    //    }
    //}

    //internal sealed class SendCantFindTvShowMessage : SendMessage
    //{
    //    public SendCantFindTvShowMessage(IStateContext stateContext) : base(stateContext, "Can't find tv show for you")
    //    {
    //    }
    //}

    //internal sealed class HandleTextMessage : ChooseState
    //{
    //    public HandleTextMessage(IStateContext stateContext, ITvShowsService tvShowsService)
    //        : base(stateContext,
    //            new (Func<CancellationToken, Task<bool>>, Type)[]
    //            {
    //                (async ct => !(await tvShowsService.SearchTvShowsAsync(((Message)stateContext.UpdateContext.Update).Text.Trim(), ct)).Any(), typeof(SendCantFindTvShowMessage)),
    //                (ct => Task.FromResult(true), typeof(FindTvShowAndGenerateMessage)),
    //            })
    //    {
    //    }
    //}

    //internal sealed class FindTvShowAndGenerateMessage : DoActionAndThen<GenerateSendPhotoRequest>
    //{
    //    public FindTvShowAndGenerateMessage(IStateContext stateContext,
    //        ITvShowsService tvShowsService,
    //        DatabaseContext.DatabaseContext databaseContext) : base(stateContext,
    //        async ct =>
    //        {
    //            var messageTextString = ((Message)stateContext.UpdateContext.Update).Text.Trim();

    //            const int pageCount = 0;

    //            var tvShows = (await tvShowsService.SearchTvShowsAsync(messageTextString, ct)).ToList();

    //            var messageText = await databaseContext.MessageTexts
    //                .AddIfNotExistsAsync(new MessageText
    //                {
    //                    Text = messageTextString
    //                }, s => s.Text == messageTextString, ct);

    //            await databaseContext.SaveChangesAsync(ct);

    //            var botMessage = new BotMessage
    //            {
    //                UserId = stateContext.UpdateContext.SessionContext.User.Id,
    //                BotCommandType = null,
    //                MessageTextId = messageText.Id,
    //                MyShowsId = tvShows.First().Id,
    //                CurrentPage = pageCount,
    //                TotalPages = tvShows.Count
    //            };

    //            await databaseContext.BotMessages.AddAsync(botMessage, ct);
    //            await databaseContext.SaveChangesAsync(ct);

    //            stateContext.Attributes[typeof(BotMessage)] = botMessage;
    //        })
    //    {
    //    }
    //}

    //internal sealed class GenerateSendPhotoRequest : DoActionAndThen<GenerateKeyboardMarkup>
    //{
    //    public GenerateSendPhotoRequest(IStateContext stateContext, ITvShowsService tvShowsService) : base(stateContext,
    //        async ct =>
    //        {
    //            const string notFoundImage = "https://images-na.ssl-images-amazon.com/images/I/312yeogBelL._SX466_.jpg";

    //            var message = (BotMessage)stateContext.Attributes[typeof(BotMessage)];

    //            var show = await tvShowsService.GetTvShowByMyShowsIdAsync(message.MyShowsId, ct)
    //                       ?? await tvShowsService.GetTvShowFromMyShowsAsync(message.MyShowsId, ct);

    //            if (string.IsNullOrEmpty(show.Image))
    //            {
    //                show.Image = notFoundImage;
    //            }

    //            var keyboardMarkup = InlineKeyboardMarkup.Empty();

    //            var request = new SendPhotoRequest(stateContext.UpdateContext.SessionContext.User.Id, show.Image)
    //            {
    //                Caption = $"{show.Title} / {show.TitleOriginal}",
    //                ReplyMarkup = keyboardMarkup
    //            };

    //            stateContext.Attributes[typeof(SendPhotoRequest)] = request;
    //            stateContext.Attributes[typeof(InlineKeyboardMarkup)] = keyboardMarkup;
    //            stateContext.Attributes[typeof(TvShow)] = show;
    //        })
    //    {
    //    }
    //}

    //internal sealed class GenerateKeyboardMarkup : DoActionAndThen<GenerateNavigationButtons>
    //{
    //    public GenerateKeyboardMarkup(IStateContext stateContext) : base(stateContext,
    //        ct => Task.CompletedTask)
    //    {
    //    }
    //}

    //internal sealed class GenerateNavigationButtons : DoActionAndThen<GenerateSubscriptionsButtons>
    //{
    //    public GenerateNavigationButtons(IStateContext stateContext, DatabaseContext.DatabaseContext databaseContext) : base(stateContext,
    //        async ct =>
    //        {
    //            var message = (BotMessage)stateContext.Attributes[typeof(BotMessage)];

    //            var keyboardMarkup = (InlineKeyboardMarkup)stateContext.Attributes[typeof(InlineKeyboardMarkup)];

    //            var buttons = new List<InlineKeyboardButton>();

    //            if (message.CurrentPage > 0)
    //            {
    //                var callback = await CreateCallbackAsync(message.Id, CallbackType.Prev, ct);
    //                buttons.Add(InlineKeyboardButton.WithCallbackData("Prev", callback.Id.ToString()));
    //            }

    //            if (message.CurrentPage < message.TotalPages - 1)
    //            {
    //                var callback = await CreateCallbackAsync(message.Id, CallbackType.Next, ct);
    //                buttons.Add(InlineKeyboardButton.WithCallbackData("Next", callback.Id.ToString()));
    //            }

    //            keyboardMarkup.AddRow(buttons);

    //            async Task<Callback> CreateCallbackAsync(int botMessageId, CallbackType callbackType, CancellationToken cancellationToken)
    //            {
    //                var callback = (await databaseContext.Callbacks
    //                    .AddAsync(new Callback
    //                    {
    //                        BotMessageId = botMessageId,
    //                        CallbackType = callbackType
    //                    }, cancellationToken)).Entity;
    //                await databaseContext.SaveChangesAsync(cancellationToken);

    //                return callback;
    //            }
    //        })
    //    {
    //    }
    //}

    //internal sealed class GenerateSubscriptionsButtons : DoActionAndThen<SendSendPhotoRequest>
    //{
    //    public GenerateSubscriptionsButtons(IStateContext stateContext, DatabaseContext.DatabaseContext databaseContext, ITvShowsService tvShowsService) : base(stateContext,
    //        async ct =>
    //        {
    //            var message = (BotMessage)stateContext.Attributes[typeof(BotMessage)];

    //            var keyboardMarkup = (InlineKeyboardMarkup)stateContext.Attributes[typeof(InlineKeyboardMarkup)];

    //            var show = (TvShow)stateContext.Attributes[typeof(TvShow)];

    //            var buttons = new List<InlineKeyboardButton>();

    //            var subscription = await tvShowsService.GetUserSubscriptionToTvShowAsync(stateContext.UpdateContext.SessionContext.User, show, SubscriptionType.EndOfShow, ct);

    //            if (subscription != null)
    //            {
    //                var callback = await CreateCallbackAsync(message.Id, CallbackType.UnsubscribeToEndOfShow, ct);
    //                buttons.Add(InlineKeyboardButton.WithCallbackData("Unsubscribe from end of show", callback.Id.ToString()));
    //            }
    //            else
    //            {
    //                var callback = await CreateCallbackAsync(message.Id, CallbackType.SubscribeToEndOfShow, ct);
    //                buttons.Add(InlineKeyboardButton.WithCallbackData("Subscribe to end of show", callback.Id.ToString()));
    //            }

    //            keyboardMarkup.AddRow(buttons);

    //            async Task<Callback> CreateCallbackAsync(int botMessageId, CallbackType callbackType, CancellationToken cancellationToken)
    //            {
    //                var callback = (await databaseContext.Callbacks
    //                    .AddAsync(new Callback
    //                    {
    //                        BotMessageId = botMessageId,
    //                        CallbackType = callbackType
    //                    }, cancellationToken)).Entity;
    //                await databaseContext.SaveChangesAsync(cancellationToken);

    //                return callback;
    //            }
    //        })
    //    {
    //    }
    //}

    //internal sealed class SendSendPhotoRequest : DoActionAndThen<HandleUpdate>
    //{
    //    public SendSendPhotoRequest(IStateContext stateContext) : base(stateContext,
    //        async ct =>
    //        {
    //            var request = (SendPhotoRequest)stateContext.Attributes[typeof(SendPhotoRequest)];

    //            await stateContext.UpdateContext.SessionContext.PostRequestAsync(request, ct);
    //        })
    //    {
    //    }
    //}

    internal sealed class Finish : StateBase {}
}
