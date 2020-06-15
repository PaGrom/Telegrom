using Autofac;
using Telegrom.Core;
using Telegrom.Core.Extensions;

namespace Telegrom.StateMachine
{
    public class StateMachineModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<UpdateHandler>()
                .As<IUpdateHandler>()
                .InstancePerUpdate();

            builder.RegisterType<StateContext>()
                .As<IStateContext>()
                .InstancePerUpdate();

            builder.RegisterType<StateMachineContext>()
                .As<IStateMachineContext>()
                .InstancePerUpdate();

            builder.RegisterType<SessionStateAttributesService>()
                .As<ISessionStateAttributesService>()
                .InstancePerUpdate();
        }
    }
}
