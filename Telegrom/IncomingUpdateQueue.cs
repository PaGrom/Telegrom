using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telegrom.Core.MessageBus;
using Telegrom.Core.TelegramModel;

namespace Telegrom
{
    internal class GlobalIncomingUpdateQueue : IGlobalIncomingUpdateQueueWriter, IGlobalIncomingUpdateQueueReader, IDisposable
    {
        private readonly IChannelReaderProvider<Update> _channelReaderProvider;
        private readonly IChannelWriterProvider<Update> _channelWriterProvider;

        public GlobalIncomingUpdateQueue()
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

        public IAsyncEnumerable<Update> DequeueAllAsync(CancellationToken cancellationToken)
        {
            return _channelReaderProvider.Reader.ReadAllAsync(cancellationToken);
        }

        public void Dispose()
        {
            _channelWriterProvider.Writer.Complete();
        }
    }
}
