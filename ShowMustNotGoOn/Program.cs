﻿using System.IO;
using System.Configuration;
using System.Threading.Tasks;
using Autofac;
using Serilog;
using ShowMustNotGoOn.Core.MessageBus;
using ShowMustNotGoOn.MyShowsRepository;
using Telegram.Bot;

namespace ShowMustNotGoOn
{
    public static class Program
    {
        private static IContainer CompositionRoot()
        {
            var builder = new ContainerBuilder();

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

            builder.RegisterType<Application>();

            return builder.Build();
        }
        public static async Task Main()
        {
            using var scope = CompositionRoot().BeginLifetimeScope();
            await scope.Resolve<Application>().RunAsync();
        }
    }
}
