using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ShowMustNotGoOn.Core.Model;
using ShowMustNotGoOn.Core.States;
using Telegrom.Core.TelegramModel;
using Telegrom.StateMachine;
using Telegrom.StateMachine.Builder;

namespace ShowMustNotGoOn
{
    public static class StateMachineBuilderFactory
    {
        public static StateMachineBuilder Create()
        {
            var stateMachineBuilder = new StateMachineBuilder();

            var initStateNode = stateMachineBuilder.AddInit<Start>();

            var (sendWelcomeMessageState, _) = initStateNode
                .SetNext(
                    NextStateKind.AfterHandle,
                    new IfState(
                        ctx => Task.FromResult(ctx.UpdateContext.Update is Message message
                                               && message.IsCommand()
                                               && string.Equals(message.Text, "/start",
                                                   StringComparison.InvariantCultureIgnoreCase)),
                        typeof(SendWelcomeMessage)),
                    new DefaultState(initStateNode));

            var defaultHandleUpdateState = sendWelcomeMessageState
                .SetNext(
                    NextStateKind.AfterOnEnter,
                    new DefaultState(
                        typeof(HandleUpdate)));

            var (handleMessageState, handleCallbackQueryState, _) = defaultHandleUpdateState
                .SetNext(
                    NextStateKind.AfterHandle,
                    new IfState(
                        ctx => Task.FromResult(ctx.UpdateContext.Update is Message),
                        typeof(HandleMessage)),
                    new IfState(
                        ctx => Task.FromResult(ctx.UpdateContext.Update is CallbackQuery),
                        typeof(HandleCallbackQuery)),
                    new DefaultState(defaultHandleUpdateState));

            var (handleCommandState, findTvShowsState) = handleMessageState
                .SetNext(
                    NextStateKind.AfterOnEnter,
                    new IfState(
                        ctx => Task.FromResult(((Message) ctx.UpdateContext.Update).IsCommand()),
                        typeof(HandleCommand)),
                    new DefaultState(
                        typeof(FindTvShows)));

            var (generateTvShowsMessageState, sendCantFindTvShowsMessageState) = findTvShowsState
                .SetNext(
                    NextStateKind.AfterOnEnter,
                    new IfState(
                        ctx =>
                        {
                            var (_, value) = ctx.Attributes[nameof(FindTvShows.TvShowsInfos)];
                            var tvShows = (List<TvShowInfo>) value;
                            return Task.FromResult(tvShows.Any());
                        },
                        typeof(GenerateTvShowsBotMessage)),
                    new DefaultState(
                        typeof(SendCantFindTvShowsMessage)));

            sendCantFindTvShowsMessageState
                .SetNext(NextStateKind.AfterOnEnter, new DefaultState(defaultHandleUpdateState));

            generateTvShowsMessageState
                .SetNext(NextStateKind.AfterOnEnter, new DefaultState(typeof(GenerateSendPhotoWithFirstTvShowRequest)))
                .SetNext(NextStateKind.AfterOnEnter, new DefaultState(typeof(GenerateKeyboard), $"{nameof(GenerateKeyboard)}ForTvShowMessage"))
                .SetNext(NextStateKind.AfterOnEnter, new DefaultState(typeof(GenerateNavigationButtons), $"{nameof(GenerateNavigationButtons)}ForTvShowMessage"))
                .SetNext(NextStateKind.AfterOnEnter, new DefaultState(typeof(GenerateSubscriptionsButtons), $"{nameof(GenerateSubscriptionsButtons)}ForTvShowMessage"))
                .SetNext(NextStateKind.AfterOnEnter, new DefaultState(typeof(SendSendPhotoRequest)))
                .SetNext(NextStateKind.AfterOnEnter, new DefaultState(defaultHandleUpdateState));

            var (handleNextCallbackQueryState, handlePrevCallbackQueryState, _) = handleCallbackQueryState
                .SetNext(
                    NextStateKind.AfterOnEnter,
                    new IfState(
                        ctx =>
                        {
                            var (_, value) = ctx.Attributes[nameof(HandleCallbackQuery.Callback)];
                            var callback = (Callback) value;
                            return Task.FromResult(callback.CallbackType == CallbackType.Next);
                        },
                        typeof(HandleNextCallbackQuery)),
                    new IfState(
                        ctx =>
                        {
                            var (_, value) = ctx.Attributes[nameof(HandleCallbackQuery.Callback)];
                            var callback = (Callback) value;
                            return Task.FromResult(callback.CallbackType == CallbackType.Prev);
                        },
                        typeof(HandlePrevCallbackQuery)),
                    new DefaultState(defaultHandleUpdateState));

            var updateTvShowsBotMessageState = handleNextCallbackQueryState
                .SetNext(
                    NextStateKind.AfterOnEnter,
                    new DefaultState(typeof(UpdateTvShowsBotMessage)));

            var generateKeyBoardForUpdateState = updateTvShowsBotMessageState
                .SetNext(
                    NextStateKind.AfterOnEnter,
                    new DefaultState(typeof(GenerateKeyboard)));

            var generateNavigationButtonsForUpdateState = generateKeyBoardForUpdateState
                .SetNext(
                    NextStateKind.AfterOnEnter,
                    new DefaultState(typeof(GenerateNavigationButtons)));

            var generateSubscriptionsButtonsForUpdateState = generateNavigationButtonsForUpdateState
                .SetNext(
                    NextStateKind.AfterOnEnter,
                    new DefaultState(typeof(GenerateSubscriptionsButtons)));

            var sendUpdatePhotoRequestState = generateSubscriptionsButtonsForUpdateState
                .SetNext(
                    NextStateKind.AfterOnEnter,
                    new DefaultState(typeof(SendUpdatePhotoRequest)));

            sendUpdatePhotoRequestState
                .SetNext(
                    NextStateKind.AfterOnEnter,
                    new DefaultState(defaultHandleUpdateState));

            handlePrevCallbackQueryState
                .SetNext(
                    NextStateKind.AfterOnEnter,
                    new DefaultState(updateTvShowsBotMessageState));

            stateMachineBuilder.SetDefaultStateNode(defaultHandleUpdateState);

            return stateMachineBuilder;
        }
    }
}
