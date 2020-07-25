using System.Collections.Generic;
using System.Linq;
using Autofac;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using ShowMustNotGoOn.Settings;
using ShowMustNotGoOn.TvShowsService;
using Telegrom;
using Telegrom.Database.InMemory;
using Telegrom.Database.Sqlite;
using Telegrom.StateMachine;
using Telegrom.TelegramService;

namespace ShowMustNotGoOn
{
    public class ContainerConfiguration
    {
        internal static void Init(IConfiguration configuration, ContainerBuilder builder)
        {
            var appSettings = new AppSettings
            {
                DatabaseSettings = configuration.GetSection("Database").Get<DatabaseSettings>(),
                TelegramSettings = configuration.GetSection("Telegram").Get<TelegramSettings>(),
                MyShowsSettings = configuration.GetSection("MyShows").Get<MyShowsSettings>()
            };

            builder.RegisterInstance(appSettings).SingleInstance();

            builder.RegisterModule(new TvShowsServiceModule
            {
                MyShowsApiUrl = appSettings.MyShowsSettings.MyShowsApiUrl,
                ProxyAddress = appSettings.MyShowsSettings.ProxyAddress
            });

            builder.Register(ctx => new MapperConfiguration(cfg =>
            {
                var profiles = ctx.Resolve<IEnumerable<Profile>>().ToList();
                foreach (var profile in profiles)
                {
                    cfg.AddProfile(profile);
                }
            }));

            builder.Register(ctx => ctx.Resolve<MapperConfiguration>().CreateMapper())
                .As<IMapper>()
                .InstancePerLifetimeScope();

            var stateMachineBuilder = StateMachineBuilderFactory.Create();

            TelegromConfiguration.Configuration
                .AddTelegramOptions(new TelegramOptions
                {
                    TelegramApiToken = appSettings.TelegramSettings.TelegramApiToken,
                    ProxyAddress = appSettings.TelegramSettings.ProxyAddress,
                    Socks5HostName = appSettings.TelegramSettings.Socks5HostName,
                    Socks5Port = appSettings.TelegramSettings.Socks5Port,
                    Socks5Username = appSettings.TelegramSettings.Socks5Username,
                    Socks5Password = appSettings.TelegramSettings.Socks5Password
                })
                //.UseInMemoryDatabase("123", optionsBuilder => optionsBuilder.EnableSensitiveDataLogging())
                .UseSqliteDatabase(appSettings.DatabaseSettings.ConnectionString, optionsBuilder => optionsBuilder.EnableSensitiveDataLogging())
                .AddStateMachineBuilder(stateMachineBuilder);
        }
    }
}
