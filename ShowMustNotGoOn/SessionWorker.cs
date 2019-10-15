using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using ShowMustNotGoOn.Core.MessageBus;

namespace ShowMustNotGoOn
{
    public sealed class SessionWorker : IDisposable
    {
        private readonly ILifetimeScope _lifetimeScope;
        private readonly IChannelReaderProvider<IMessage> _channelReaderProvider;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public SessionWorker(ILifetimeScope lifetimeScope,
            IChannelReaderProvider<IMessage> channelReaderProvider,
            CancellationTokenSource cancellationTokenSource)
        {
            _cancellationTokenSource = cancellationTokenSource;
            _lifetimeScope = lifetimeScope;
            _channelReaderProvider = channelReaderProvider;
        }

        public async Task Start()
        {
            try
            {
                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    var message = await _channelReaderProvider.Reader.ReadAsync(_cancellationTokenSource.Token);

                    if (!_cancellationTokenSource.IsCancellationRequested)
                    {
                        using var innerScope =
                            _lifetimeScope.BeginLifetimeScope(ContainerConfiguration.RequestLifetimeScopeTag);

                        await innerScope.Resolve<MessageHandler>().HandleAsync(message);
                    }
                }
            }
            catch (OperationCanceledException)
            {}

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