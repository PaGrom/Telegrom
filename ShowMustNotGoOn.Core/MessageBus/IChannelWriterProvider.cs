using System.Threading.Channels;

namespace ShowMustNotGoOn.Core.MessageBus
{
    public interface IChannelWriterProvider<T>
    {
        ChannelWriter<T> Writer { get; }
    }
}