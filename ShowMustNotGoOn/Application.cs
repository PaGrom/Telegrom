using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Autofac;
using Microsoft.EntityFrameworkCore;
using Serilog;
using ShowMustNotGoOn.Core;
using ShowMustNotGoOn.Core.MessageBus;
using ShowMustNotGoOn.Core.Model;
using ShowMustNotGoOn.Core.Model.Callback;
using ShowMustNotGoOn.Messages.Events;

namespace ShowMustNotGoOn
{
    public class Application
    {
        private readonly Dispatcher _dispatcher;
        private readonly ILogger _logger;

        public Application(Dispatcher dispatcher,
            ITelegramService telegramService,
            ILogger logger)
        {
            _dispatcher = dispatcher;
            _logger = logger;

            telegramService.SetMessageReceivedHandler(HandleTelegramMessageReceived);
            telegramService.SetCallbackButtonReceivedHandler(HandleCallbackButtonReceived);
            telegramService.Start();

            Task.Factory.StartNew(async () => { await RunAsync(); },
                TaskCreationOptions.LongRunning);
        }

        public async Task RunAsync()
        {
            _logger.Information("Application start");
            while (true)
            {
                await Task.Delay(int.MaxValue);
            }
        }

        public async void HandleTelegramMessageReceived(UserMessage userMessage)
        {
            await _dispatcher.WriteAsync(new TelegramMessageReceivedEvent(userMessage));
        }

        private async void HandleCallbackButtonReceived(CallbackButton callbackButton)
        {
            await _dispatcher.WriteAsync(new TelegramCallbackButtonReceivedEvent(callbackButton));
        }
    }

    public sealed class Dispatcher : IDisposable
    {
        private readonly ILifetimeScope _lifetimeScope;
        private readonly object _syncRoot = new object();
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly Dictionary<int, ILifetimeScope> _sessionScopes = new Dictionary<int, ILifetimeScope>();
        private readonly Dictionary<int, Task> _sessionTasks = new Dictionary<int, Task>();

        public Dispatcher(ILifetimeScope lifetimeScope)
        {
            _lifetimeScope = lifetimeScope;
        }

        public async Task WriteAsync(IMessage message)
        {
            if (!_sessionScopes.TryGetValue(message.UserId, out var scope))
            {
                lock (_syncRoot)
                {
                    if (!_sessionScopes.TryGetValue(message.UserId, out scope))
                    {
                        scope = _lifetimeScope.BeginLifetimeScope(ContainerConfiguration.SessionLifetimeScopeTag,
                            builder =>
                            {
                                builder.RegisterInstance(message);
                            });

                        _sessionScopes[message.UserId] = scope;
                        var messageHandler = scope.Resolve<ChannelWorker>();
                        var task = Task.Run(messageHandler.Start, _cancellationTokenSource.Token);
                        _sessionTasks[message.UserId] = task;
                    }
                }
            }
            var channelWriter = scope.Resolve<IChannelWriterProvider<IMessage>>();
            await channelWriter.Writer.WriteAsync(message);
        }

        public void Dispose()
        {
            if (!_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
            }

            foreach (var (sessionId, scope) in _sessionScopes)
            {
                scope.Dispose();
                _sessionTasks[sessionId].GetAwaiter().GetResult();
            }
        }
    }

    public interface IChannelWriterProvider<T>
    {
        ChannelWriter<T> Writer { get; }
    }

    public interface IChannelReaderProvider<T>
    {
        ChannelReader<T> Reader { get; }
    }

    public class ChannelHolder<T> : IChannelReaderProvider<T>, IChannelWriterProvider<T>
    {
        private readonly Channel<T> _channel;

        public ChannelHolder()
        {
            _channel = Channel.CreateUnbounded<T>();
        }

        public ChannelReader<T> Reader => _channel.Reader;
        public ChannelWriter<T> Writer => _channel.Writer;
    }

    public sealed class ChannelWorker : IDisposable
    {
        private readonly ILifetimeScope _lifetimeScope;
        private readonly IChannelReaderProvider<IMessage> _channelReaderProvider;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public ChannelWorker(ILifetimeScope lifetimeScope, IChannelReaderProvider<IMessage> channelReaderProvider)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _lifetimeScope = lifetimeScope;
            _channelReaderProvider = channelReaderProvider;
        }

        public async Task Start()
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                var message = await _channelReaderProvider.Reader.ReadAsync(_cancellationTokenSource.Token);

                if (!_cancellationTokenSource.IsCancellationRequested)
                {
                    using var innerScope = _lifetimeScope.BeginLifetimeScope(ContainerConfiguration.RequestLifetimeScopeTag);

                    await innerScope.Resolve<MessageHandler>().HandleAsync();
                }
            }

            Console.WriteLine($"CommandHandler stopped");
        }

        public void Dispose()
        {
            if (_cancellationTokenSource.IsCancellationRequested)
                return;

            _cancellationTokenSource.Cancel();
        }
    }
}
