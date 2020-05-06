using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using ShowMustNotGoOn.Core.TelegramModel;

namespace ShowMustNotGoOn.Core.Contexts
{
    public sealed class SessionManager : IAsyncDisposable
    {
        private readonly ILifetimeScope _lifetimeScope;
        private readonly IUsersService _usersService;
        private readonly object _syncRoot = new object();
        private readonly LinkedList<int> _recentSessionContextsQueue = new LinkedList<int>();
        private readonly Dictionary<int, SessionContext> _sessionContexts = new Dictionary<int, SessionContext>();
        private const int MaxActiveSessions = 1;

        public SessionManager(ILifetimeScope lifetimeScope, IUsersService usersService)
        {
            _lifetimeScope = lifetimeScope;
            _usersService = usersService;
        }

        public async Task<SessionContext> GetSessionContextAsync(User user, CancellationToken cancellationToken)
        {
            await _usersService.AddOrUpdateUserAsync(user, cancellationToken);

            SessionContext removedSession = null;

            if (!_sessionContexts.TryGetValue(user.Id, out var sessionContext))
            {
                lock (_syncRoot)
                {
                    if (!_sessionContexts.TryGetValue(user.Id, out sessionContext))
                    {
                        var scope = _lifetimeScope.BeginLifetimeScope(
                            typeof(SessionContext),
                            builder =>
                            {
                                builder.RegisterInstance(user);
                            });

                        sessionContext = scope.Resolve<SessionContext>();

                        sessionContext.Start(cancellationToken);

                        _sessionContexts.Add(user.Id, sessionContext);

                        _recentSessionContextsQueue.AddFirst(user.Id);
                        while (_recentSessionContextsQueue.Count > MaxActiveSessions)
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
