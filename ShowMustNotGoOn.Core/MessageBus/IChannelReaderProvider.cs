using System.Threading.Channels;

namespace ShowMustNotGoOn.Core.MessageBus
{
    public interface IChannelReaderProvider<T>
    {
        ChannelReader<T> Reader { get; }
    }
}