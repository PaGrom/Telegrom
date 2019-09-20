using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Serilog;
using ShowMustNotGoOn.Core.MessageBus;

namespace ShowMustNotGoOn
{
    public sealed class MessageBus : IMessageBus, IDisposable
    {
        private readonly ILogger _logger;
        private readonly ChannelWriter<IMessage> _writer;
        private readonly Dictionary<Type, Delegate> _handlers = new Dictionary<Type, Delegate>();

        private readonly ReaderWriterLockSlim _readerWriterLock =
            new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public MessageBus(ILogger logger)
        {
            _logger = logger;

            var channel = Channel.CreateUnbounded<IMessage>();
            var reader = channel.Reader;
            _writer = channel.Writer;

            Task.Factory.StartNew(async () =>
            {
                while (await reader.WaitToReadAsync())
                {
                    try
                    {
                        _readerWriterLock.EnterReadLock();
                        var message = await reader.ReadAsync();
                        var handlerExists = _handlers.TryGetValue(message.GetType(), out var value);
                        if (!handlerExists)
                        {
                            continue;
                        }
                        value.DynamicInvoke(message);
                    }
                    finally
                    {
                        _readerWriterLock.ExitReadLock();
                    }
                }
            }, TaskCreationOptions.LongRunning);
        }

        public void RegisterHandler<T>(Action<T> handleAction) where T : IMessage
        {
            var messageType = typeof(T);
            try
            {
                _readerWriterLock.EnterUpgradeableReadLock();
                if (!_handlers.TryGetValue(messageType, out var handler))
                {
                    try
                    {
                        _readerWriterLock.EnterWriteLock();
                        if (!_handlers.TryGetValue(messageType, out handler))
                        {
                            _handlers.Add(messageType, handleAction);
                        }
                        else
                        {
                            var typedHandler = (Action<T>)handler;
                            typedHandler += handleAction;
                            _handlers[messageType] = typedHandler;
                        }
                    }
                    finally
                    {
                        _readerWriterLock.ExitWriteLock();
                    }
                }
                else
                {
                    try
                    {
                        _readerWriterLock.EnterWriteLock();
                        var typedHandler = (Action<T>)handler;
                        typedHandler += handleAction;
                        _handlers[messageType] = typedHandler;
                    }
                    finally
                    {
                        _readerWriterLock.ExitWriteLock();
                    }
                }
            }
            finally
            {
                _readerWriterLock.ExitUpgradeableReadLock();
            }

            _logger.Information("Handler registered: {@handler}", handleAction);
        }

        [SuppressMessage("ReSharper", "DelegateSubtraction")]
        public void UnregisterHandler<T>(Action<T> handleAction) where T : IMessage
        {
            var messageType = typeof(T);
            try
            {
                _readerWriterLock.EnterUpgradeableReadLock();
                if (!_handlers.TryGetValue(messageType, out var handler))
                {
                    return;
                }

                try
                {
                    _readerWriterLock.EnterWriteLock();
                    var typedHandler = (Action<T>)handler;
                    typedHandler -= handleAction;
                    if (typedHandler != null)
                    {
                        _handlers[messageType] = typedHandler;
                    }
                    else
                    {
                        _handlers.Remove(messageType);
                    }
                }
                finally
                {
                    _readerWriterLock.ExitWriteLock();
                }
            }
            finally
            {
                _readerWriterLock.ExitUpgradeableReadLock();
            }

            _logger.Information("Handler unregistered: {@handler}", handleAction);
        }

        public async Task Enqueue(IMessage message)
        {
            _logger.Information("Message enqueued: {@message}", message);
            await _writer.WriteAsync(message);
        }

        public void Stop()
        {
            _writer.Complete();
        }

        public void Dispose()
        {
            Stop();
            _readerWriterLock.Dispose();
        }
    }
}
