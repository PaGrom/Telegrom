using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegrom.Core.MessageBus;

namespace Telegrom
{
    internal class SessionIncomingUpdateQueue : ISessionIncomingUpdateQueueWriter, ISessionIncomingUpdateQueueReader, IDisposable
    {
        private readonly IChannelReaderProvider<Update> _channelReaderProvider;
        private readonly IChannelWriterProvider<Update> _channelWriterProvider;

        public SessionIncomingUpdateQueue()
        {
            var channelHolder = new ChannelHolder<Update>();
            _channelReaderProvider = channelHolder;
            _channelWriterProvider = channelHolder;
        }

        public ValueTask EnqueueAsync(Update update, CancellationToken cancellationToken)
        {
            return _channelWriterProvider.Writer.WriteAsync(update, cancellationToken);
        }

        public ValueTask<Update> DequeueAsync(CancellationToken cancellationToken)
        {
            return _channelReaderProvider.Reader.ReadAsync(cancellationToken);
        }

        public void Complete()
        {
            _channelWriterProvider.Writer.Complete();
        }

        public void Dispose()
        {
            Complete();
        }
    }
}
