using System;
using Autofac;
using Autofac.Builder;
using Telegrom.Core.Extensions;

namespace Telegrom.StateMachine
{
    public static class AutofacExtensions
    {
        public static IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle>
            RegisterState(this ContainerBuilder builder, Type type)
        {
            return builder.RegisterType(type)
                .Named<IState>(type.Name)
                .InstancePerUpdate();
        }
    }
}
