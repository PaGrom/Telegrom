using System.Threading.Channels;

namespace Telegrom.Core.MessageBus
{
    internal sealed class ChannelHolder<T> : IChannelReaderProvider<T>, IChannelWriterProvider<T>
    {
        private readonly Channel<T> _channel;

        public ChannelHolder()
        {
            _channel = Channel.CreateUnbounded<T>();
        }

        public ChannelReader<T> Reader => _channel.Reader;
        public ChannelWriter<T> Writer => _channel.Writer;
    }
}
