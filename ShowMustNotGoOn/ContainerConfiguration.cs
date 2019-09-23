using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using Autofac;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Serilog;
using ShowMustNotGoOn.DatabaseService;
using ShowMustNotGoOn.MessageBus;
using ShowMustNotGoOn.Messages.Handlers;
using ShowMustNotGoOn.MyShowsService;
using ShowMustNotGoOn.TelegramService;

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

            builder.RegisterModule(new TelegramServiceModule
            {
                TelegramApiToken = ConfigurationManager.AppSettings["TelegramApiToken"]
            });

            builder.RegisterModule(new MyShowsRepositoryModule
            {
                MyShowsApiUrl = ConfigurationManager.AppSettings["MyShowsApiUrl"]
            });

            builder.RegisterModule(new DatabaseRepositoryModule
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString
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

            builder.RegisterType<DatabaseMessageHandler>()
                .SingleInstance()
                .AutoActivate();

            builder.RegisterType<MyShowsMessageHandler>()
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
