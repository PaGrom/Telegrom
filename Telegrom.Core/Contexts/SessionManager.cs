using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Telegrom.Core.TelegramModel;

namespace Telegrom.Core.Contexts
{
    public sealed class SessionManager : IAsyncDisposable
    {
        private readonly ILifetimeScope _lifetimeScope;
        private readonly IIdentityService _identityService;
        private readonly object _syncRoot = new object();
        private readonly LinkedList<int> _recentSessionContextsQueue = new LinkedList<int>();
        private readonly Dictionary<int, SessionContext> _sessionContexts = new Dictionary<int, SessionContext>();
        private readonly int _maxActiveSessions;

        public SessionManager(ILifetimeScope lifetimeScope, IIdentityService identityService, int maxActiveSessions)
        {
            _lifetimeScope = lifetimeScope;
            _identityService = identityService;
            _maxActiveSessions = maxActiveSessions;
        }

        public async Task<SessionContext> GetSessionContextAsync(User user, CancellationToken cancellationToken)
        {
            await _identityService.AddOrUpdateUserAsync(user, cancellationToken);

            SessionContext removedSession = null;

            if (!_sessionContexts.TryGetValue(user.Id, out var sessionContext))
            {
                lock (_syncRoot)
                {
                    if (!_sessionContexts.TryGetValue(user.Id, out sessionContext))
                    {
                        var scope = _lifetimeScope.BeginLifetimeScope(
                            typeof(SessionContext),
                            builder => { builder.RegisterInstance(user); });

                        sessionContext = scope.Resolve<SessionContext>();

                        sessionContext.Start(cancellationToken);

                        _sessionContexts.Add(user.Id, sessionContext);

                        _recentSessionContextsQueue.AddFirst(user.Id);
                        while (_recentSessionContextsQueue.Count > _maxActiveSessions)
                        {
                            var sessionId = _recentSessionContextsQueue.Last.Value;
                            _recentSessionContextsQueue.RemoveLast();
                            _sessionContexts.Remove(sessionId, out removedSession);
                        }
                    }
                }
            }

            if (removedSession != null)
            {
                await removedSession.Complete();
            }

            return sessionContext;
        }

        public async Task CompleteAllAsync()
        {
            await Task.WhenAll(_sessionContexts.Values.Select(sessionContext => sessionContext.Complete()));
        }

        public async ValueTask DisposeAsync()
        {
            await CompleteAllAsync();
        }
    }
}
