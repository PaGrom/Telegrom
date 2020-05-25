using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using ShowMustNotGoOn.Core.TelegramModel;
using ShowMustNotGoOn.DatabaseContext.Model;
using ShowMustNotGoOn.StateMachine;
using ShowMustNotGoOn.StateMachine.Builder;
using ShowMustNotGoOn.States;

namespace ShowMustNotGoOn
{
    public class StateMachineBuilderModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var stateMachineBuilder = new StateMachineBuilder(builder);

            var initStateNode = stateMachineBuilder.AddInit<Start>();

            var (sendWelcomeMessageState, _) = initStateNode
                .SetNext(
                    NextStateKind.AfterHandle,
                    ctx => Task.FromResult(ctx.UpdateContext.Update is Message message
                                           && message.IsCommand()
                                           && string.Equals(message.Text, "/start",
                                               StringComparison.InvariantCultureIgnoreCase)),
                    typeof(SendWelcomeMessage),
                    initStateNode);

            var defaultHandleUpdateState = sendWelcomeMessageState
                .SetNext<HandleUpdate>(NextStateKind.AfterOnEnter);

            var (handleMessageState, _) = defaultHandleUpdateState
                .SetNext(
                    NextStateKind.AfterHandle,
                    ctx => Task.FromResult(ctx.UpdateContext.Update is Message),
                    typeof(HandleMessage),
                    defaultHandleUpdateState);

            var (handleCommandState, findTvShowsState) = handleMessageState
                .SetNext<HandleCommand, FindTvShows>(
                    NextStateKind.AfterOnEnter,
                    ctx => Task.FromResult(((Message)ctx.UpdateContext.Update).IsCommand()));

            var (generateTvShowsMessageState, sendCantFindTvShowsMessageState) = findTvShowsState
                .SetNext<GenerateTvShowsBotMessage, SendCantFindTvShowsMessage>(
                    NextStateKind.AfterOnEnter,
                    ctx =>
                    {
                        var (_, value) = ctx.Attributes[nameof(FindTvShows.TvShows)];
                        var tvShows = (List<TvShow>)value;
                        return Task.FromResult(tvShows.Any());
                    });

            sendCantFindTvShowsMessageState.SetNext(NextStateKind.AfterOnEnter, defaultHandleUpdateState);

            generateTvShowsMessageState
                .SetNext<GenerateSendPhotoWithFirstTvShowRequest>(NextStateKind.AfterOnEnter)
                .SetNext<GenerateKeyboard>(NextStateKind.AfterOnEnter)
                .SetNext<GenerateNavigationButtons>(NextStateKind.AfterOnEnter)
                .SetNext<GenerateSubscriptionsButtons>(NextStateKind.AfterOnEnter)
                .SetNext<SendSendPhotoRequest>(NextStateKind.AfterOnEnter)
                .SetNext(NextStateKind.AfterOnEnter, defaultHandleUpdateState);

            stateMachineBuilder.SetDefaultStateNode(defaultHandleUpdateState);

            stateMachineBuilder.Build();

            builder.RegisterInstance(new StateMachineConfigurationProvider(stateMachineBuilder.InitStateName, stateMachineBuilder.DefaultStateName))
                .As<IStateMachineConfigurationProvider>();
        }
    }
}
