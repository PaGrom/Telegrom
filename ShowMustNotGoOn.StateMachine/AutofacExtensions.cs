using System;
using Autofac;
using Autofac.Builder;
using ShowMustNotGoOn.Core.Extensions;

namespace ShowMustNotGoOn.StateMachine
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
