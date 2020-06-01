using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShowMustNotGoOn.Core;
using ShowMustNotGoOn.Core.TelegramModel;
using ShowMustNotGoOn.DatabaseContext.Model;
using ShowMustNotGoOn.StateMachine;

namespace ShowMustNotGoOn.States
{
    internal sealed class HandleCallbackQuery : StateBase
    {
        private readonly IStateContext _stateContext;
        private readonly DatabaseContext.DatabaseContext _databaseContext;
        private readonly ITvShowsService _tvShowsService;

        public HandleCallbackQuery(IStateContext stateContext,
            DatabaseContext.DatabaseContext databaseContext,
            ITvShowsService tvShowsService)
        {
            _stateContext = stateContext;
            _databaseContext = databaseContext;
            _tvShowsService = tvShowsService;
        }

        public override async Task OnEnter(CancellationToken cancellationToken)
        {
            var callbackQuery = _stateContext.UpdateContext.Update as CallbackQuery;

            var callbackId = Guid.Parse(callbackQuery.Data);

            var callback = await _databaseContext.Callbacks.FindAsync(new object[] { callbackId }, cancellationToken);

            var botMessage = await _databaseContext.BotMessages
                .FindAsync(new object[] { callback.BotMessageId }, cancellationToken);

            //if (botMessage.BotCommandType == BotCommandType.Subscriptions)
            //{
            //    await HandleSubscriptionsCommandAsync(callbackQuery, callback, botMessage, cancellationToken);
            //    return;
            //}

            var messageText = await _databaseContext.MessageTexts
                .FindAsync(new object[] { botMessage.MessageTextId }, cancellationToken);

            var tvShows = (await _tvShowsService.SearchTvShowsAsync(messageText.Text, cancellationToken)).ToList();

            var currentShow = await _tvShowsService.GetTvShowByMyShowsIdAsync(botMessage.MyShowsId, cancellationToken)
                              ?? await _tvShowsService.GetTvShowFromMyShowsAsync(botMessage.MyShowsId, cancellationToken);

            switch (callback.CallbackType)
            {
                case CallbackType.Next:
                    botMessage.CurrentPage++;
                    break;
                case CallbackType.Prev:
                    botMessage.CurrentPage--;
                    break;
                case CallbackType.SubscribeToEndOfShow:
                    await _tvShowsService.SubscribeUserToTvShowAsync(_stateContext.UpdateContext.SessionContext.User,
                        currentShow,
                        SubscriptionType.EndOfShow,
                        cancellationToken);
                    break;
                case CallbackType.UnsubscribeToEndOfShow:
                    await _tvShowsService.UnsubscribeUserFromTvShowAsync(_stateContext.UpdateContext.SessionContext.User,
                        currentShow,
                        SubscriptionType.EndOfShow,
                        cancellationToken);
                    break;
                default:
                    return;
            }

            botMessage.MyShowsId = tvShows[botMessage.CurrentPage].Id;

            _databaseContext.BotMessages.Update(botMessage);
            await _databaseContext.SaveChangesAsync(cancellationToken);

            //await UpdateMessageAsync(_updateContext.SessionContext.User,
            //    botMessage,
            //    callbackQuery.MessageId,
            //    callbackQuery.Id,
            //    cancellationToken);
        }
    }
}
