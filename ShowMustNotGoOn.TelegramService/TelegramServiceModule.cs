using System.Net;
using Autofac;
using AutoMapper;
using ShowMustNotGoOn.Core;
using Telegram.Bot;

namespace ShowMustNotGoOn.TelegramService
{
    public class TelegramServiceModule : Module
    {
        public string TelegramApiToken { get; set; }
        public string ProxyAddress { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            if (string.IsNullOrEmpty(ProxyAddress))
            {
                builder.RegisterInstance(new TelegramBotClient(TelegramApiToken))
                    .AsImplementedInterfaces();
            }
            else
            {
                builder.RegisterInstance(new TelegramBotClient(TelegramApiToken,
                        new WebProxy(ProxyAddress) { UseDefaultCredentials = true }))
                    .AsImplementedInterfaces();
            }

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
//, new WebProxy("xz.avp.ru:8080"){UseDefaultCredentials = true}