using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using Autofac;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Serilog;
using ShowMustNotGoOn.DatabaseContext;
using ShowMustNotGoOn.MessageBus;
using ShowMustNotGoOn.Messages.Handlers;
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
                        Path.Combine(ConfigurationManager.AppSettings["LogFolder"], "Log-{Date}.txt"))
                    .CreateLogger())
                .SingleInstance();

            builder.RegisterModule<MessageBusModule>();

            builder.RegisterModule(new DatabaseContextModule
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString
            });

            builder.RegisterModule<UsersServiceModule>();

            builder.RegisterModule(new TvShowsServiceModule
            {
                MyShowsApiUrl = ConfigurationManager.AppSettings["MyShowsApiUrl"]
            });

            builder.RegisterModule(new TelegramServiceModule
            {
                TelegramApiToken = ConfigurationManager.AppSettings["TelegramApiToken"]
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
