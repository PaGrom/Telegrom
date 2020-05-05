using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using ShowMustNotGoOn.Core.MessageBus;
using ShowMustNotGoOn.Core.Request;
using ShowMustNotGoOn.Core.TelegramModel;

namespace ShowMustNotGoOn.Core.Session
{
    public sealed class SessionContext
    {
        private readonly ILifetimeScope _lifetimeScope;
        private readonly IChannelReaderProvider<Update> _incomingChannelReaderProvider;
        private readonly IChannelWriterProvider<Update> _incomingChannelWriterProvider;
        private readonly ILogger<SessionContext> _logger;
        private Task _handleTask;

        public User User { get; }

        public SessionContext(ILifetimeScope lifetimeScope,
            User user,
            IChannelReaderProvider<Update> incomingChannelReaderProvider,
            IChannelWriterProvider<Update> incomingChannelWriterProvider,
            ILogger<SessionContext> logger)
        {
            _lifetimeScope = lifetimeScope;
            User = user;
            _incomingChannelReaderProvider = incomingChannelReaderProvider;
            _incomingChannelWriterProvider = incomingChannelWriterProvider;
            _logger = logger;
        }

        public void Start(CancellationToken cancellationToken)
        {
            async Task Handle()
            {
                await foreach (var update in _incomingChannelReaderProvider.Reader.ReadAllAsync(cancellationToken))
                {
                    var requestContext = new RequestContext(this, update);
                    await using var innerScope = _lifetimeScope.BeginLifetimeScope(
                        typeof(RequestContext),
                        builder =>
                        {
                            builder.RegisterInstance(requestContext);
                        });

                    await innerScope.Resolve<MessageHandler>().HandleAsync(cancellationToken);
                }
            }

            _logger.LogInformation($"Session for identityUser {User.Id} started");

            _handleTask = Task.Run(Handle, cancellationToken);
        }

        public async Task PostUpdateAsync(Update update, CancellationToken cancellationToken)
        {
            await _incomingChannelWriterProvider.Writer.WriteAsync(update, cancellationToken);
        }

        public async Task Complete()
        {
            _incomingChannelWriterProvider.Writer.Complete();
            if (_handleTask != null)
            {
                await _handleTask;
            }
            _logger.LogInformation($"Session for identityUser {User.Id} stopped");
        }
    }
}
