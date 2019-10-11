using System.Threading.Channels;

namespace ShowMustNotGoOn
{
    public interface IChannelReaderProvider<T>
    {
        ChannelReader<T> Reader { get; }
    }
}