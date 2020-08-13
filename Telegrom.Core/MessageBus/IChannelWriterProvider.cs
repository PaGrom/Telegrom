using System.Threading.Channels;

namespace Telegrom.Core.MessageBus
{
    internal interface IChannelWriterProvider<T>
    {
        ChannelWriter<T> Writer { get; }
    }
}