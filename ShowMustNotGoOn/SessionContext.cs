using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Serilog;
using ShowMustNotGoOn.Core.MessageBus;
using ShowMustNotGoOn.Core.Model;

namespace ShowMustNotGoOn
{
    public sealed class SessionContext
    {
        private readonly ILifetimeScope _lifetimeScope;
        private readonly User _user;
        private readonly IChannelReaderProvider<IMessage> _channelReaderProvider;
        private readonly IChannelWriterProvider<IMessage> _channelWriterProvider;
        private readonly ILogger _logger;
        private Task _handleTask;

        public SessionContext(ILifetimeScope lifetimeScope,
	        User user,
	        IChannelReaderProvider<IMessage> channelReaderProvider,
	        IChannelWriterProvider<IMessage> channelWriterProvider,
	        ILogger logger)
        {
            _lifetimeScope = lifetimeScope;
            _user = user;
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
			        await using var innerScope = _lifetimeScope.BeginLifetimeScope(ContainerConfiguration.RequestLifetimeScopeTag);

			        await innerScope.Resolve<MessageHandler>().HandleAsync(_user, message);
		        }
	        }

            _logger.Information($"Session for user {_user.Id} started");

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
            _logger.Information($"Session for user {_user.Id} stopped");
        }
    }
}
