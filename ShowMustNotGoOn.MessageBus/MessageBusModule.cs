using Autofac;
using ShowMustNotGoOn.Core.MessageBus;

namespace ShowMustNotGoOn.MessageBus
{
    public class MessageBusModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<MessageBus>()
                .As<IMessageBus>()
                .SingleInstance();
        }
    }
}
