using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telegrom.Core.MessageBus;
using Telegrom.Core.TelegramModel;

namespace Telegrom
{
    internal sealed class SessionOutgoingRequestQueue : ISessionOutgoingRequestQueueWriter, ISessionOutgoingRequestQueueReader, IDisposable
    {
        private readonly IChannelReaderProvider<RequestBase> _channelReaderProvider;
        private readonly IChannelWriterProvider<RequestBase> _channelWriterProvider;

        public SessionOutgoingRequestQueue()
        {
            var channelHolder = new ChannelHolder<RequestBase>();
            _channelReaderProvider = channelHolder;
            _channelWriterProvider = channelHolder;
        }

        public ValueTask EnqueueAsync(RequestBase requestBase, CancellationToken cancellationToken)
        {
            return _channelWriterProvider.Writer.WriteAsync(requestBase, cancellationToken);
        }

        public ValueTask<RequestBase> DequeueAsync(CancellationToken cancellationToken)
        {
            return _channelReaderProvider.Reader.ReadAsync(cancellationToken);
        }

        public void Dispose()
        {
            _channelWriterProvider.Writer.Complete();
        }
    }
}
