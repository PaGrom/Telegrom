﻿using System.Net;
using Autofac;
using AutoMapper;
using MihaZupan;
using ShowMustNotGoOn.Core;
using Telegram.Bot;

namespace ShowMustNotGoOn.TelegramService
{
    public class TelegramServiceModule : Module
    {
        public string TelegramApiToken { get; set; }
        public string ProxyAddress { get; set; }
        public string Socks5HostName { get; set; }
        public int? Socks5Port { get; set; }
        public string Socks5Username { get; set; }
        public string Socks5Password { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            if (!string.IsNullOrEmpty(ProxyAddress))
            {
                builder.RegisterInstance(new TelegramBotClient(TelegramApiToken, new WebProxy(ProxyAddress)
                    {
                        UseDefaultCredentials = true
                    }))
                    .AsImplementedInterfaces();
            }
            else if (!string.IsNullOrEmpty(Socks5HostName)
                && Socks5Port.HasValue)
            {
                HttpToSocks5Proxy proxy;
                if (!string.IsNullOrEmpty(Socks5Username)
                    && !string.IsNullOrEmpty(Socks5Password))
                {
                    proxy = new HttpToSocks5Proxy(Socks5HostName, Socks5Port.Value, Socks5Username, Socks5Password);
                }
                else
                {
                    proxy = new HttpToSocks5Proxy(Socks5HostName, Socks5Port.Value);
                }

                builder.RegisterInstance(new TelegramBotClient(TelegramApiToken, proxy))
                    .AsImplementedInterfaces();
            }
            else
            {
                builder.RegisterInstance(new TelegramBotClient(TelegramApiToken))
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
