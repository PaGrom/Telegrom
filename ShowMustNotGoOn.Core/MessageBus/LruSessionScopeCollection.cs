using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;

namespace ShowMustNotGoOn.Core.MessageBus
{
    public class LruSessionScopeCollection : IDisposable
    {
        private class SessionScopeNode : IDisposable
        {
            public ILifetimeScope SessionScope { get; }
            private Task SessionTask { get; }
            private readonly CancellationTokenSource _cancellationTokenSource;

            public SessionScopeNode(ILifetimeScope sessionScope, Task sessionTask, CancellationTokenSource cancellationTokenSource)
            {
                SessionScope = sessionScope;
                SessionTask = sessionTask;
                _cancellationTokenSource = cancellationTokenSource;
            }

            public void Dispose()
            {
                _cancellationTokenSource.Cancel();
                SessionTask.GetAwaiter().GetResult();
                SessionScope.Dispose();
                _cancellationTokenSource.Dispose();
            }
        }

        private readonly int _capacity;
        private readonly Dictionary<int, SessionScopeNode> _dictionary = new Dictionary<int, SessionScopeNode>();
        private readonly LinkedList<int> _sessionScopesQueue = new LinkedList<int>();
        private readonly ReaderWriterLockSlim _readerWriterLock = new ReaderWriterLockSlim();

        public LruSessionScopeCollection(int capacity)
        {
            if (capacity < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }
            _capacity = capacity;
        }

        public void Add(int sessionId, ILifetimeScope sessionScope, Task sessionTask, CancellationTokenSource cancellationTokenSource)
        {
            _readerWriterLock.EnterWriteLock();
            if (_dictionary.Count == _capacity)
            {
                var minUsageSessionScopeKey = _sessionScopesQueue.Last();
                var minUsageSessionScope = _dictionary[minUsageSessionScopeKey];
                _dictionary.Remove(minUsageSessionScopeKey);
                _sessionScopesQueue.RemoveLast();
                minUsageSessionScope.Dispose();
            }
            _dictionary[sessionId] = new SessionScopeNode(sessionScope, sessionTask, cancellationTokenSource);
            _sessionScopesQueue.AddFirst(sessionId);
            _readerWriterLock.ExitWriteLock();
        }

        public bool ContainsSessionScopeForSessionId(int sessionId)
        {
            return _dictionary.ContainsKey(sessionId);
        }

        public bool RemoveSessionScopeBySessionId(int sessionId)
        {
            _readerWriterLock.EnterWriteLock();
            var scope = _dictionary[sessionId];
            var result = _dictionary.Remove(sessionId);
            scope.Dispose();
            _readerWriterLock.ExitWriteLock();
            return result;
        }

        public bool TryGetSessionScope(int sessionId, out ILifetimeScope sessionScope)
        {
            _readerWriterLock.EnterReadLock();
            var result = _dictionary.TryGetValue(sessionId, out var nodeValue);
            if (result)
            {
                sessionScope = nodeValue.SessionScope;
                _sessionScopesQueue.Remove(sessionId);
                _sessionScopesQueue.AddFirst(sessionId);
            }
            else
            {
                sessionScope = default;
            }
            _readerWriterLock.ExitReadLock();
            return result;
        }

        public void Clear()
        {
            _readerWriterLock.EnterWriteLock();
            foreach (var (_, sessionScope) in _dictionary)
            {
                sessionScope.Dispose();
            }
            _dictionary.Clear();
            _sessionScopesQueue.Clear();
            _readerWriterLock.ExitWriteLock();
        }

        public void Dispose()
        {
            Clear();
            _readerWriterLock?.Dispose();
        }
    }
}
