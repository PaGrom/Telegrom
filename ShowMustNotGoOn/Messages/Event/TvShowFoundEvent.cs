using ShowMustNotGoOn.Core;
using ShowMustNotGoOn.Core.MessageBus;

namespace ShowMustNotGoOn.Messages.Event
{
    public sealed class TvShowFoundEvent : IMessage
    {
        public TvShow TvShow { get; }

        public TvShowFoundEvent(TvShow tvShow)
        {
            TvShow = tvShow;
        }
    }
}
