using ShowMustNotGoOn.Core;
using ShowMustNotGoOn.Core.MessageBus;
using ShowMustNotGoOn.Core.Model;

namespace ShowMustNotGoOn.Messages.Commands
{
    public sealed class AddTvShowToDbCommand : IMessage
    {
        public TvShow TvShow { get; }

        public AddTvShowToDbCommand(TvShow tvShow)
        {
            TvShow = tvShow;
        }
    }
}
