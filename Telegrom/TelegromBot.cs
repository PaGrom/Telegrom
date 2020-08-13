using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Telegrom.Core;
using Telegrom.Core.Contexts;
using Telegrom.Core.Extensions;
using Telegrom.Core.MessageBus;
using Telegrom.Core.TelegramModel;
using Telegrom.Database;
using Telegrom.StateMachine;
using Telegrom.TelegramService;

namespace Telegrom
{
    public class TelegromBot : BackgroundService, IAsyncDisposable
    {
        private readonly ILifetimeScope _lifetimeScope;

        public TelegromClient TelegromClient { get; }

        public TelegromBot(ILifetimeScope rootLifetimeScope = null)
        {
            _ = StateMachineBuilder.Current ?? throw new Exception("You have to create StateMachineBuilder");

            _ = DatabaseOptions.Current ?? throw new Exception("You have to configure db");

            rootLifetimeScope ??= new ContainerBuilder().Build();

            _lifetimeScope = rootLifetimeScope.BeginLifetimeScope(Configure);

            TelegromClient = _lifetimeScope.Resolve<TelegromClient>();
        }

        protected override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            return _lifetimeScope.Resolve<InternalBotService>().RunAsync(cancellationToken);
        }

        private static void Configure(ContainerBuilder builder)
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

            var options = DatabaseOptions.Current
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

            builder.RegisterModule<TelegramServiceModule>();

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

            StateMachineBuilder.Current.Build(builder);

            builder.RegisterInstance(new StateMachineConfigurationProvider(StateMachineBuilder.Current.InitStateName,
                    StateMachineBuilder.Current.DefaultStateName))
                .As<IStateMachineConfigurationProvider>();

            builder.RegisterType<TelegromClient>()
                .SingleInstance();

            builder.RegisterType<InternalBotService>()
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

            static bool NotNetFramework(string assemblyName)
            {
                return !assemblyName.StartsWith("Microsoft.")
                       && !assemblyName.StartsWith("Windows.")
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

        public ValueTask DisposeAsync()
        {
            return _lifetimeScope.DisposeAsync();
        }
    }
}
