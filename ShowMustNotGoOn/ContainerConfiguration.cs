using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using Autofac;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Serilog;
using ShowMustNotGoOn.DatabaseContext;
using ShowMustNotGoOn.MessageBus;
using ShowMustNotGoOn.Messages.Handlers;
using ShowMustNotGoOn.Settings;
using ShowMustNotGoOn.TelegramService;
using ShowMustNotGoOn.TvShowsService;
using ShowMustNotGoOn.UsersService;

namespace ShowMustNotGoOn
{
    public class ContainerConfiguration
    {
        internal static void Init(IConfiguration configuration, ContainerBuilder builder)
        {
            builder.Register<ILogger>((c, p) => new LoggerConfiguration()
                    .MinimumLevel.Information()
                    .WriteTo.Console()
                    .WriteTo.RollingFile(
                        Path.Combine(configuration.GetSection("LogFolder").Value, "Log-{Date}.txt"))
                    .CreateLogger())
                .SingleInstance();

            var appSettings = new AppSettings
            {
                DatabaseSettings = configuration.GetSection("Database").Get<DatabaseSettings>(),
                TelegramSettings = configuration.GetSection("Telegram").Get<TelegramSettings>(),
                MyShowsSettings = configuration.GetSection("MyShows").Get<MyShowsSettings>(),
                GlobalSettings = configuration.GetSection("Global").Get<GlobalSettings>()
            };
            
            builder.RegisterInstance(appSettings).SingleInstance();

            builder.RegisterModule<MessageBusModule>();

            builder.RegisterModule(new DatabaseContextModule
            {
                ConnectionString = appSettings.DatabaseSettings.ConnectionString
            });

            builder.RegisterModule<UsersServiceModule>();

            builder.RegisterModule(new TvShowsServiceModule
            {
                MyShowsApiUrl = appSettings.MyShowsSettings.MyShowsApiUrl,
                ProxyAddress = appSettings.GlobalSettings.ProxyAddress
            });

            builder.RegisterModule(new TelegramServiceModule
            {
                TelegramApiToken = appSettings.TelegramSettings.TelegramApiToken,
                ProxyAddress = appSettings.GlobalSettings.ProxyAddress
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

            builder.RegisterType<UsersMessageHandler>()
                .SingleInstance()
                .AutoActivate();

            builder.RegisterType<TvShowsMessageHandler>()
                .SingleInstance()
                .AutoActivate();

            builder.RegisterType<TelegramMessageHandler>()
                .SingleInstance()
                .AutoActivate();

            builder.RegisterType<Application>()
                .AsSelf()
                .SingleInstance()
                .AutoActivate();
        }
    }
}
