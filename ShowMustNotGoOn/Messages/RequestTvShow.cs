using ShowMustNotGoOn.Core;
using ShowMustNotGoOn.Core.MessageBus;

namespace ShowMustNotGoOn.Messages
{
    public sealed class SaveTvShowToDb : IMessage
    {
        public TvShow TvShow { get; set; }
    }
}
