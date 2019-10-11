using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using ShowMustNotGoOn.Core.MessageBus;

namespace ShowMustNotGoOn
{
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
                        var messageHandler = scope.Resolve<SessionWorker>();
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
}