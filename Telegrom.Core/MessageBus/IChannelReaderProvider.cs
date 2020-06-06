using System.Threading.Channels;

namespace Telegrom.Core.MessageBus
{
    public interface IChannelReaderProvider<T>
    {
        ChannelReader<T> Reader { get; }
    }
}