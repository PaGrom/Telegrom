using System;
using Autofac;
using ShowMustNotGoOn.Core;
using ShowMustNotGoOn.Core.Extensions;

namespace ShowMustNotGoOn.StateMachine
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
        }
    }
}
