using ShowMustNotGoOn.Core;
using ShowMustNotGoOn.Core.MessageBus;
using ShowMustNotGoOn.Core.Model;

namespace ShowMustNotGoOn.Messages.Event
{
    public sealed class TvShowAddedToDbEvent : IMessage
    {
        public TvShow TvShow { get; }

        public TvShowAddedToDbEvent(TvShow tvShow)
        {
            TvShow = tvShow;
        }
    }
}
