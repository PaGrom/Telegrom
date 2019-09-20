using ShowMustNotGoOn.Core.MessageBus;

namespace ShowMustNotGoOn.Messages.Commands
{
    public sealed class SearchTvShowByNameCommand : IMessage
    {
        public string Name { get; }
        public int Position { get; }

        public SearchTvShowByNameCommand(string name, int position = default)
        {
            Name = name;
            Position = position;
        }
    }
}
