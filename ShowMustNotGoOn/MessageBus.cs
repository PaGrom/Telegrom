using System;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;
using ShowMustNotGoOn.Core.MessageBus;

namespace ShowMustNotGoOn
{
    public sealed class MessageBus : IMessageBus, IDisposable
    {
        private readonly ChannelWriter<IMessage> _writer;
        private readonly Dictionary<Type, Action<IMessage>> _handlers = new Dictionary<Type, Action<IMessage>>();

        public MessageBus()
        {
            var channel = Channel.CreateUnbounded<IMessage>();
            var reader = channel.Reader;
            _writer = channel.Writer;

            Task.Factory.StartNew(async () =>
            {
                while (await reader.WaitToReadAsync())
                {
                    var job = await reader.ReadAsync();
                    var handlerExists = _handlers.TryGetValue(job.GetType(), out var value);
                    if (handlerExists)
                    {
                        value.Invoke(job);
                    }
                }
            }, TaskCreationOptions.LongRunning);
        }

        public void RegisterHandler<T>(Action<T> handleAction) where T : IMessage
        {
            void ActionWrapper(IMessage job) => handleAction((T) job);
            _handlers.Add(typeof(T), ActionWrapper);
        }

        public async Task Enqueue(IMessage job)
        {
            await _writer.WriteAsync(job);
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
