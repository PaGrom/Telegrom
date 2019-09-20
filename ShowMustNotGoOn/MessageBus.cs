using System;
using System.Collections.Generic;
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
        private readonly Dictionary<Type, Action<IMessage>> _handlers = new Dictionary<Type, Action<IMessage>>();

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
                    var message = await reader.ReadAsync();
                    var handlerExists = _handlers.TryGetValue(message.GetType(), out var value);
                    if (handlerExists)
                    {
                        value.Invoke(message);
                    }
                }
            }, TaskCreationOptions.LongRunning);
        }

        public void RegisterHandler<T>(Action<T> handleAction) where T : IMessage
        {
            void ActionWrapper(IMessage job) => handleAction((T) job);
            _handlers.Add(typeof(T), ActionWrapper);
            _logger.Information("Handler registered: {@handler}", handleAction);
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
        }
    }
}
