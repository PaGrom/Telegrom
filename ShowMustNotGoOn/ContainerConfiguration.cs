using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using ShowMustNotGoOn.Core.Contexts;
using ShowMustNotGoOn.Core.Extensions;
using ShowMustNotGoOn.Core.MessageBus;
using ShowMustNotGoOn.Core.TelegramModel;
using ShowMustNotGoOn.Settings;
using ShowMustNotGoOn.StateMachine;
using ShowMustNotGoOn.StateMachine.Builder;
using ShowMustNotGoOn.States;
using ShowMustNotGoOn.TelegramService;
using ShowMustNotGoOn.TvShowsService;
using ShowMustNotGoOn.UsersService;

namespace ShowMustNotGoOn
{
    public class ContainerConfiguration
    {
        internal static void Init(IConfiguration configuration, ContainerBuilder builder)
        {
            var loggerFactory = LoggerFactory.Create(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddConsole(consoleOptions =>
                {
                    consoleOptions.Format = ConsoleLoggerFormat.Default;
                    consoleOptions.TimestampFormat = "[HH:mm:ss] ";
                });
                loggingBuilder.SetMinimumLevel(LogLevel.Trace);
            });
            builder.RegisterInstance(loggerFactory);
            builder.RegisterGeneric(typeof(Logger<>))
                .As(typeof(ILogger<>))
                .SingleInstance();

            var appSettings = new AppSettings
            {
                DatabaseSettings = configuration.GetSection("Database").Get<DatabaseSettings>(),
                TelegramSettings = configuration.GetSection("Telegram").Get<TelegramSettings>(),
                MyShowsSettings = configuration.GetSection("MyShows").Get<MyShowsSettings>()
            };
            
            builder.RegisterInstance(appSettings).SingleInstance();

            var options = new DbContextOptionsBuilder<DatabaseContext.DatabaseContext>()
                .UseSqlite(appSettings.DatabaseSettings.ConnectionString)
                .EnableSensitiveDataLogging()
                .UseLoggerFactory(loggerFactory)
                .Options;

            builder.RegisterInstance(options)
                .As<DbContextOptions>();

            builder.RegisterType<DatabaseContext.DatabaseContext>()
                .InstancePerUpdate();

            builder.RegisterModule<UsersServiceModule>();

            builder.RegisterModule(new TvShowsServiceModule
            {
                MyShowsApiUrl = appSettings.MyShowsSettings.MyShowsApiUrl,
                ProxyAddress = appSettings.MyShowsSettings.ProxyAddress
            });

            builder.RegisterModule(new TelegramServiceModule
            {
                TelegramApiToken = appSettings.TelegramSettings.TelegramApiToken,
                ProxyAddress = appSettings.TelegramSettings.ProxyAddress,
                Socks5HostName = appSettings.TelegramSettings.Socks5HostName,
                Socks5Port = appSettings.TelegramSettings.Socks5Port,
                Socks5Username = appSettings.TelegramSettings.Socks5Username,
                Socks5Password = appSettings.TelegramSettings.Socks5Password
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

            builder.RegisterType<SessionManager>()
                .InstancePerLifetimeScope();

            builder.RegisterType<MessageHandler>()
                .InstancePerUpdate();

            builder.RegisterType<SessionContext>()
                .InstancePerSession();

            builder.RegisterType<ChannelHolder<Update>>()
                .As<IChannelReaderProvider<Update>>()
                .As<IChannelWriterProvider<Update>>()
                .InstancePerSession();

            builder.RegisterType<ChannelHolder<Request>>()
	            .As<IChannelReaderProvider<Request>>()
	            .As<IChannelWriterProvider<Request>>()
	            .InstancePerSession();

            builder.RegisterModule<StateMachineModule>();

            var states = Assembly.GetCallingAssembly().GetTypes()
                .Where(type => !type.IsAbstract && typeof(IState).IsAssignableFrom(type));

            foreach (var state in states)
            {
                builder.RegisterState(state);
            }

            var stateMachineBuilder = new StateMachineBuilder(builder);

            var initStateNode = stateMachineBuilder.AddInit<Start>();
            initStateNode.AddNext<SendWelcomeMessage>(NextStateType.AfterHandle)
                .AddNext(initStateNode, NextStateType.AfterOnEnter);

            stateMachineBuilder.SetDefaultStateNode(initStateNode);

            stateMachineBuilder.Build();

            builder.RegisterInstance(new StateMachineConfigurationProvider(stateMachineBuilder.InitStateName, stateMachineBuilder.DefaultStateName))
                .As<IStateMachineConfigurationProvider>();

            builder.RegisterType<Application>()
                .AsSelf()
                .SingleInstance()
                .AutoActivate();
        }
    }
}
