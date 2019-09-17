using System.Collections.Generic;
using System.IO;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using AutoMapper;
using DbRepository;
using Serilog;
using ShowMustNotGoOn.Core.MessageBus;
using ShowMustNotGoOn.MyShowsRepository;
using ShowMustNotGoOn.MyShowsRepository.Model;
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
