using System;
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
        private readonly LruSessionScopeCollection _sessionScopes = new LruSessionScopeCollection(1);

        public Dispatcher(ILifetimeScope lifetimeScope)
        {
            _lifetimeScope = lifetimeScope;
        }

        public async Task WriteAsync(IMessage message)
        {
            if (!_sessionScopes.TryGetSessionScope(message.UserId, out var scope))
            {
                lock (_syncRoot)
                {
                    if (!_sessionScopes.TryGetSessionScope(message.UserId, out scope))
                    {
                        scope = _lifetimeScope.BeginLifetimeScope(ContainerConfiguration.SessionLifetimeScopeTag,
                            builder =>
                            {
                                builder.RegisterInstance(message);
                            });

                        var cancellationTokenSource = new CancellationTokenSource();
                        var messageHandler = scope.Resolve<SessionWorker>(
                            new TypedParameter(typeof(CancellationTokenSource), cancellationTokenSource));
                        var task = Task.Run(messageHandler.Start, cancellationTokenSource.Token);
                        _sessionScopes.Add(message.UserId, scope, task, cancellationTokenSource);
                    }
                }
            }
            var channelWriter = scope.Resolve<IChannelWriterProvider<IMessage>>();
            await channelWriter.Writer.WriteAsync(message);
        }

        public void Dispose()
        {
            _sessionScopes.Dispose();
        }
    }
}