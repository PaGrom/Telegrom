using System.Threading.Channels;

namespace ShowMustNotGoOn
{
    public interface IChannelWriterProvider<T>
    {
        ChannelWriter<T> Writer { get; }
    }
}