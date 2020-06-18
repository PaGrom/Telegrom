using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using ShowMustNotGoOn.Settings;
using ShowMustNotGoOn.TvShowsService;
using Telegrom.Core;
using Telegrom.Core.Contexts;
using Telegrom.Core.Extensions;
using Telegrom.Core.MessageBus;
using Telegrom.Core.TelegramModel;
using Telegrom.Database;
using Telegrom.StateMachine;
using Telegrom.TelegramService;

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

            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseSqlite(appSettings.DatabaseSettings.ConnectionString)
                .EnableSensitiveDataLogging()
                .UseLoggerFactory(loggerFactory)
                .Options;

            builder.RegisterInstance(options)
                .As<DbContextOptions>();

            builder.RegisterType<DatabaseContext>()
                .InstancePerUpdate();

            builder.RegisterType<WakeUpService>()
                .As<IWakeUpService>();

            builder.RegisterType<IdentityService>()
                .As<IIdentityService>();

            builder.RegisterType<IdentityStatesService>()
                .As<IIdentityStatesService>()
                .InstancePerUpdate();

            builder.RegisterType<GlobalAttributesService>()
                .As<IGlobalAttributesService>();

            builder.RegisterType<SessionAttributesService>()
                .As<ISessionAttributesService>()
                .InstancePerUpdate();

            builder.RegisterType<SessionStateAttributesRemover>()
                .As<ISessionStateAttributesRemover>();

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

            //builder.RegisterType<MessageHandler>()
            //    .InstancePerUpdate();

            builder.RegisterType<SessionContext>()
                .InstancePerSession();

            builder.RegisterType<DatabaseContext>()
                .InstancePerSession();

            builder.RegisterType<UpdateService>()
                .As<IUpdateService>()
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

            LoadAllAssemblies();

            var states = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes())
                .Where(type => !type.IsAbstract && type.IsPublic && typeof(IState).IsAssignableFrom(type))
                .ToList();

            foreach (var state in states)
            {
                builder.RegisterState(state);
            }

            builder.RegisterModule<StateMachineBuilderModule>();

            builder.RegisterType<Application>()
                .AsSelf()
                .SingleInstance()
                .AutoActivate();
        }

        // Source: https://dotnetstories.com/blog/Dynamically-pre-load-assemblies-in-a-ASPNET-Core-or-any-C-project-en-7155735300

        private static void LoadAllAssemblies(bool includeFramework = false)
        {
            // Storage to ensure not loading the same assembly twice and optimize calls to GetAssemblies()
            var loaded = new ConcurrentDictionary<string, bool>();

            // Filter to avoid loading all the .net framework
            bool ShouldLoad(string assemblyName)
            {
                return (includeFramework || NotNetFramework(assemblyName))
                       && !loaded.ContainsKey(assemblyName);
            }

            bool NotNetFramework(string assemblyName)
            {
                return !assemblyName.StartsWith("Microsoft.")
                       && !assemblyName.StartsWith("System.")
                       && !assemblyName.StartsWith("Newtonsoft.")
                       && assemblyName != "netstandard";
            }

            void LoadReferencedAssembly(Assembly assembly)
            {
                // Check all referenced assemblies of the specified assembly
                foreach (var an in assembly.GetReferencedAssemblies().Where(a => ShouldLoad(a.FullName)))
                {
                    // Load the assembly and load its dependencies
                    LoadReferencedAssembly(Assembly.Load(an)); // AppDomain.CurrentDomain.Load(name)
                    loaded.TryAdd(an.FullName, true);
                    System.Diagnostics.Debug.WriteLine($"\n>> Referenced assembly => {an.FullName}");
                }
            }

            // Populate already loaded assemblies
            System.Diagnostics.Debug.WriteLine($">> Already loaded assemblies:");
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies().Where(a => ShouldLoad(a.FullName)))
            {
                loaded.TryAdd(a.FullName, true);
                System.Diagnostics.Debug.WriteLine($">>>> {a.FullName}");
            }

            int alreadyLoaded = loaded.Keys.Count();
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

            // Loop on loaded assembliesto load dependencies (it includes Startup assembly so should load all the dependency tree) 
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(a => NotNetFramework(a.FullName)))
                LoadReferencedAssembly(assembly);

            // Debug
            System.Diagnostics.Debug.WriteLine(
                $"\n>> Assemblies loaded after scann ({(loaded.Keys.Count - alreadyLoaded)} assemblies in {sw.ElapsedMilliseconds} ms):");
            foreach (var a in loaded.Keys.OrderBy(k => k))
                System.Diagnostics.Debug.WriteLine($">>>> {a}");
        }
    }
}
