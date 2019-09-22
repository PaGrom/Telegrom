using Autofac;
using AutoMapper;
using ShowMustNotGoOn.Core;
using Telegram.Bot;

namespace ShowMustNotGoOn.TelegramService
{
    public class TelegramServiceModule : Module
    {
        public string TelegramApiToken { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(new TelegramBotClient(TelegramApiToken))
                .AsImplementedInterfaces();

            builder.RegisterType<TelegramServiceMappingProfile>()
                .As<Profile>()
                .InstancePerLifetimeScope();

            builder.RegisterType<TelegramService>()
                .As<ITelegramService>()
                .SingleInstance()
                .AutoActivate();
        }
    }
}
