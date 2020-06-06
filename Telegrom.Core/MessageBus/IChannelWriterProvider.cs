using System.Threading.Channels;

namespace Telegrom.Core.MessageBus
{
    public interface IChannelWriterProvider<T>
    {
        ChannelWriter<T> Writer { get; }
    }
}