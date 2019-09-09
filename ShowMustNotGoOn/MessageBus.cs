using System;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;
using ShowMustNotGoOn.Core.MessageBus;

namespace ShowMustNotGoOn
{
    public sealed class MessageBus : IMessageBus
    {
        private readonly ChannelWriter<IJob> _writer;
        private readonly Dictionary<Type, Action<IJob>> _handlers = new Dictionary<Type, Action<IJob>>();

        public MessageBus()
        {
            var channel = Channel.CreateUnbounded<IJob>();
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

        public void RegisterHandler<T>(Action<T> handleAction) where T : IJob
        {
            void ActionWrapper(IJob job) => handleAction((T) job);
            _handlers.Add(typeof(T), ActionWrapper);
        }

        public async Task Enqueue(IJob job)
        {
            await _writer.WriteAsync(job);
        }

        public void Stop()
        {
            _writer.Complete();
        }
    }
}
