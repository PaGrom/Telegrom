using System.Net;
using Autofac;
using AutoMapper;
using MihaZupan;
using Telegram.Bot;
using Telegrom.Core;
using Telegrom.Core.MessageBus;

namespace Telegrom.TelegramService
{
    public class TelegramServiceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            if (!string.IsNullOrEmpty(TelegramOptions.Current.ProxyAddress))
            {
                builder.RegisterInstance(new TelegramBotClient(
                        TelegramOptions.Current.TelegramApiToken,
                        new WebProxy(TelegramOptions.Current.ProxyAddress) 
                        {
                            UseDefaultCredentials = true
                        }))
                    .AsImplementedInterfaces()
                    .SingleInstance();
            }
            else if (!string.IsNullOrEmpty(TelegramOptions.Current.Socks5HostName)
                && TelegramOptions.Current.Socks5Port.HasValue)
            {
                HttpToSocks5Proxy proxy;
                if (!string.IsNullOrEmpty(TelegramOptions.Current.Socks5Username)
                    && !string.IsNullOrEmpty(TelegramOptions.Current.Socks5Password))
                {
                    proxy = new HttpToSocks5Proxy(
                        TelegramOptions.Current.Socks5HostName,
                        TelegramOptions.Current.Socks5Port.Value,
                        TelegramOptions.Current.Socks5Username,
                        TelegramOptions.Current.Socks5Password);
                }
                else
                {
                    proxy = new HttpToSocks5Proxy(TelegramOptions.Current.Socks5HostName, TelegramOptions.Current.Socks5Port.Value);
                }

                builder.RegisterInstance(new TelegramBotClient(TelegramOptions.Current.TelegramApiToken, proxy))
                    .AsImplementedInterfaces()
                    .SingleInstance();
            }
            else
            {
                builder.RegisterInstance(new TelegramBotClient(TelegramOptions.Current.TelegramApiToken))
                    .AsImplementedInterfaces()
                    .SingleInstance();
            }

            builder.RegisterType<TelegramMappingProfile>()
                .As<Profile>()
                .InstancePerLifetimeScope();

            builder.RegisterType<TelegramUpdateReceiver>()
                .As<ITelegramUpdateReceiver>()
                .SingleInstance();
        }
    }
}
