using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telegrom.Core.MessageBus;
using Telegrom.Core.TelegramModel;

namespace Telegrom
{
    internal sealed class GlobalOutgoingRequestQueue : IGlobalOutgoingRequestQueueWriter, IGlobalOutgoingRequestQueueReader, IDisposable
    {
        private readonly IChannelReaderProvider<Request> _channelReaderProvider;
        private readonly IChannelWriterProvider<Request> _channelWriterProvider;

        public GlobalOutgoingRequestQueue()
        {
            var channelHolder = new ChannelHolder<Request>();
            _channelReaderProvider = channelHolder;
            _channelWriterProvider = channelHolder;
        }

        public ValueTask EnqueueAsync(Request request, CancellationToken cancellationToken)
        {
            return _channelWriterProvider.Writer.WriteAsync(request, cancellationToken);
        }

        public ValueTask<Request> DequeueAsync(CancellationToken cancellationToken)
        {
            return _channelReaderProvider.Reader.ReadAsync(cancellationToken);
        }

        public IAsyncEnumerable<Request> DequeueAllAsync(CancellationToken cancellationToken)
        {
            return _channelReaderProvider.Reader.ReadAllAsync(cancellationToken);
        }

        public void Dispose()
        {
            _channelWriterProvider.Writer.Complete();
        }
    }
}
