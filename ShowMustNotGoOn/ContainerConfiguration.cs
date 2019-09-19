using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using Autofac;
using AutoMapper;
using DbRepository;
using Microsoft.Extensions.Configuration;
using Serilog;
using ShowMustNotGoOn.Core;
using ShowMustNotGoOn.Core.MessageBus;
using ShowMustNotGoOn.MyShowsRepository;
using Telegram.Bot;

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

            builder.RegisterType<MessageBus>()
                .As<IMessageBus>()
                .SingleInstance();

            builder.RegisterInstance(new TelegramBotClient(ConfigurationManager.AppSettings["TelegramApiToken"]))
                .AsImplementedInterfaces();

            builder.RegisterModule(new MyShowsRepositoryModule
            {
                MyShowsApiUrl = ConfigurationManager.AppSettings["MyShowsApiUrl"]
            });

            builder.RegisterModule(new DbRepositoryModule
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

            builder.RegisterType<TelegramService>()
                .As<ITelegramService>()
                .SingleInstance()
                .AutoActivate();

            builder.RegisterType<Application>()
                .AsSelf()
                .SingleInstance()
                .AutoActivate();
        }
    }
}
