using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using Telegrom.Core.MessageBus;
using Telegrom.Core.TelegramModel;

namespace Telegrom.Core.Contexts
{
    public sealed class SessionContext
    {
        private readonly ILifetimeScope _lifetimeScope;
        private readonly ISessionIncomingUpdateQueueReader _incomingUpdateQueueReader;
        private readonly ISessionIncomingUpdateQueueWriter _incomingUpdateQueueWriter;
        private readonly ISessionOutgoingRequestQueueReader _outgoingRequestQueueReader;
        private readonly ISessionOutgoingRequestQueueWriter _outgoingRequestQueueWriter;
        private readonly IRequestDispatcher _requestDispatcher;
        private readonly ISessionStateAttributesRemover _sessionStateAttributesRemover;
        private readonly IUpdateService _updateService;
        private readonly ILogger<SessionContext> _logger;
        private Task _updateHandleTask;
        private Task _requestHandleTask;

        public User User { get; }

        public SessionContext(ILifetimeScope lifetimeScope,
            User user,
            ISessionIncomingUpdateQueueReader incomingUpdateQueueReader,
            ISessionIncomingUpdateQueueWriter incomingUpdateQueueWriter,
            ISessionOutgoingRequestQueueReader outgoingRequestQueueReader,
            ISessionOutgoingRequestQueueWriter outgoingRequestQueueWriter,
            IRequestDispatcher requestDispatcher,
            ISessionStateAttributesRemover sessionStateAttributesRemover,
            IUpdateService updateService,
            ILogger<SessionContext> logger)
        {
            _lifetimeScope = lifetimeScope;
            _incomingUpdateQueueReader = incomingUpdateQueueReader;
            _incomingUpdateQueueWriter = incomingUpdateQueueWriter;
            _outgoingRequestQueueReader = outgoingRequestQueueReader;
            _outgoingRequestQueueWriter = outgoingRequestQueueWriter;
            User = user;
            _requestDispatcher = requestDispatcher;
            _sessionStateAttributesRemover = sessionStateAttributesRemover;
            _updateService = updateService;
            _logger = logger;
        }

        public void Start(CancellationToken cancellationToken)
        {
            async Task UpdateHandle()
            {
                await foreach (var update in _incomingUpdateQueueReader.DequeueAllAsync(cancellationToken))
                {
                    await _updateService.SaveUpdateAsync(update, cancellationToken);
                    var updateContext = new UpdateContext(this, update);
                    await using var innerScope = _lifetimeScope.BeginLifetimeScope(
                        typeof(UpdateContext),
                        builder => { builder.RegisterInstance(updateContext).As<IUpdateContext>(); });

                    //await innerScope.Resolve<MessageHandler>().UpdateHandleAsync(cancellationToken);
                    await innerScope.Resolve<IUpdateHandler>().Execute(cancellationToken);
                    await _updateService.MakeUpdateProcessedAsync(update, cancellationToken);
                }
            }

            async Task RequestHandle()
            {
                await foreach (var request in _outgoingRequestQueueReader.DequeueAllAsync(cancellationToken))
                {
                    await _requestDispatcher.DispatchAsync(request, cancellationToken);
                }
            }

            async Task RestartOnExceptionAsync(Func<Task> task)
            {
                do
                {
                    try
                    {
                        await task();
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Get exception during update or request handle");
                    }
                } while (true);
            }

            _logger.LogInformation($"Session for identityUser {User.Id} started");

            _updateHandleTask = Task.Run(() => RestartOnExceptionAsync(UpdateHandle), cancellationToken);
            _requestHandleTask = Task.Run(() => RestartOnExceptionAsync(RequestHandle), cancellationToken);
        }

        public async Task PostUpdateAsync(Update update, CancellationToken cancellationToken)
        {
            await _incomingUpdateQueueWriter.EnqueueAsync(update, cancellationToken);
        }

        public async Task PostRequestAsync(Request request, CancellationToken cancellationToken)
        {
            await _outgoingRequestQueueWriter.EnqueueAsync(request, cancellationToken);
        }

        public async Task Complete()
        {
            if (_updateHandleTask != null)
            {
                await _updateHandleTask;
            }

            if (_requestHandleTask != null)
            {
                await _requestHandleTask;
            }

            await _sessionStateAttributesRemover.RemoveAllSessionStateAttributesAsync(User, CancellationToken.None);

            _logger.LogInformation($"Session for identityUser {User.Id} stopped");
        }
    }
}
