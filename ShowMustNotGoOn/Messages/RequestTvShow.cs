using ShowMustNotGoOn.Core.MessageBus;

namespace ShowMustNotGoOn.Messages
{
    public class RequestTvShow : IMessage
    {
        public string Name { get; set; }
    }
}
