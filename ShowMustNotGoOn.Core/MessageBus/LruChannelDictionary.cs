using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ShowMustNotGoOn.Core.MessageBus
{
    public class LruChannelCollection
    {
        private class ChannelNode : IDisposable
        {
            private readonly CancellationTokenSource _cancellationTokenSource;
            public Channel<IMessage> Channel { get; }

            public ChannelNode(Channel<IMessage> channel, Task readerTask, CancellationTokenSource cancellationTokenSource)
            {
                _cancellationTokenSource = cancellationTokenSource;
                Channel = channel;
            }

            public void Dispose()
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
            }
        }

        private readonly int _capacity;
        private readonly Dictionary<int, ChannelNode> _dictionary = new Dictionary<int, ChannelNode>();
        private readonly LinkedList<int> _channelsUsageQueue = new LinkedList<int>();
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
            if (_dictionary.Count == _capacity)
            {
                var minUsageChannelKey = _channelsUsageQueue.Last();
                var minUsageChannel = _dictionary[minUsageChannelKey];
                _dictionary.Remove(minUsageChannelKey);
                _channelsUsageQueue.RemoveLast();
                minUsageChannel.Dispose();
            }
            _dictionary[sessionId] = new ChannelNode(channel, readerTask, cancellationTokenSource);
            _channelsUsageQueue.AddFirst(sessionId);
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
                _channelsUsageQueue.Remove(sessionId);
                _channelsUsageQueue.AddFirst(sessionId);
            }
            else
            {
                channel = default;
            }
            _readerWriterLock.ExitReadLock();
            return result;
        }

        public void Clear()
        {
            _readerWriterLock.EnterWriteLock();
            _dictionary.Clear();
            _channelsUsageQueue.Clear();
            _readerWriterLock.ExitWriteLock();
        }
    }
}
