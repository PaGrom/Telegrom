using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using ShowMustNotGoOn.Core.MessageBus;
using ShowMustNotGoOn.Core.Request;
using ShowMustNotGoOn.DatabaseContext.Model;

namespace ShowMustNotGoOn.Core.Session
{
    public sealed class SessionContext
    {
        private readonly ILifetimeScope _lifetimeScope;
        private readonly IChannelReaderProvider<IMessage> _channelReaderProvider;
        private readonly IChannelWriterProvider<IMessage> _channelWriterProvider;
        private readonly ILogger<SessionContext> _logger;
        private Task _handleTask;

        public User User { get; }

        public SessionContext(ILifetimeScope lifetimeScope,
            User user,
            IChannelReaderProvider<IMessage> channelReaderProvider,
            IChannelWriterProvider<IMessage> channelWriterProvider,
            ILogger<SessionContext> logger)
        {
            _lifetimeScope = lifetimeScope;
            User = user;
            _channelReaderProvider = channelReaderProvider;
            _channelWriterProvider = channelWriterProvider;
            _logger = logger;
        }

        public void Start(CancellationToken cancellationToken)
        {
            async Task Handle()
            {
                await foreach (var message in _channelReaderProvider.Reader.ReadAllAsync(cancellationToken))
                {
                    var requestContext = new RequestContext(message);
                    await using var innerScope = _lifetimeScope.BeginLifetimeScope(
                        typeof(RequestContext),
                        builder =>
                        {
                            builder.RegisterInstance(requestContext);
                        });

                    await innerScope.Resolve<MessageHandler>().HandleAsync();
                }
            }

            _logger.LogInformation($"Session for user {User.Id} started");

            _handleTask = Task.Run(Handle, cancellationToken);
        }

        public async Task PostMessageAsync(IMessage message, CancellationToken cancellationToken)
        {
            await _channelWriterProvider.Writer.WriteAsync(message, cancellationToken);
        }

        public async Task Complete()
        {
            _channelWriterProvider.Writer.Complete();
            if (_handleTask != null)
            {
                await _handleTask;
            }
            _logger.LogInformation($"Session for user {User.Id} stopped");
        }
    }
}
