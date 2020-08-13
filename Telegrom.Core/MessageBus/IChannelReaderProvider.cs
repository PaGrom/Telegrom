using System.Threading.Channels;

namespace Telegrom.Core.MessageBus
{
    internal interface IChannelReaderProvider<T>
    {
        ChannelReader<T> Reader { get; }
    }
}