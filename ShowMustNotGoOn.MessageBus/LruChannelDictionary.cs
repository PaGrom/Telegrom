using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using ShowMustNotGoOn.Core.MessageBus;

namespace ShowMustNotGoOn.MessageBus
{
    public class LruChannelCollection
    {
        private class ChannelNode : IDisposable
        {
            private readonly Task _readerTask;
            private readonly CancellationTokenSource _cancellationTokenSource;
            public int UsageCount { get; private set; } = default;
            public Channel<IMessage> Channel { get; }

            public ChannelNode(Channel<IMessage> channel, Task readerTask, CancellationTokenSource cancellationTokenSource)
            {
                _readerTask = readerTask;
                _cancellationTokenSource = cancellationTokenSource;
                Channel = channel;
            }

            public void IncreaseUsageCount()
            {
                UsageCount++;
            }

            public void Dispose()
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
            }
        }

        public int Count { get; private set; } = default;
        private readonly int _capacity;
        private readonly Dictionary<int, ChannelNode> _dictionary = new Dictionary<int, ChannelNode>();
        private readonly ReaderWriterLockSlim _readerWriterLock = new ReaderWriterLockSlim();

        public LruChannelCollection(int capacity)
        {
            if (capacity < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }
            _capacity = capacity;
        }

        public void Add(int sessionId, Channel<IMessage> channel, Task readerTask, CancellationTokenSource cancellationTokenSource)
        {
            _readerWriterLock.EnterWriteLock();
            if (Count == _capacity)
            {
                var minUsageNode = _dictionary.OrderBy(x => x.Value.UsageCount).First();
                _dictionary.Remove(minUsageNode.Key);
                minUsageNode.Value.Dispose();
                Count--;
            }
            _dictionary[sessionId] = new ChannelNode(channel, readerTask, cancellationTokenSource);
            Count++;
            _readerWriterLock.ExitWriteLock();
        }

        public bool ContainsChannelForSessionId(int sessionId)
        {
            return _dictionary.ContainsKey(sessionId);
        }

        public bool RemoveChannelBySessionId(int sessionId)
        {
            _readerWriterLock.EnterWriteLock();
            var result = _dictionary.Remove(sessionId);
            if (result)
            {
                Count--;
            }
            _readerWriterLock.ExitWriteLock();
            return result;
        }

        public bool TryGetChannel(int sessionId, out Channel<IMessage> channel)
        {
            _readerWriterLock.EnterReadLock();
            var result = _dictionary.TryGetValue(sessionId, out var nodeValue);
            if (result)
            {
                channel = nodeValue.Channel;
                nodeValue.IncreaseUsageCount();
            }
            else
            {
                channel = default;
            }
            _readerWriterLock.ExitReadLock();
            return result;
        }

        public ICollection<int> SessionIds
        {
            get
            {
                _readerWriterLock.EnterReadLock();
                var keys = _dictionary.Keys;
                _readerWriterLock.ExitReadLock();
                return keys;
            }
        }

        public ICollection<Channel<IMessage>> Channels
        {
            get
            {
                _readerWriterLock.EnterReadLock();
                var values = _dictionary.Values.Select(n => n.Channel).ToList();
                _readerWriterLock.ExitReadLock();
                return values;
            }
        }

        public void Clear()
        {
            _readerWriterLock.EnterWriteLock();
            _dictionary.Clear();
            Count = 0;
            _readerWriterLock.ExitWriteLock();
        }
    }
}
