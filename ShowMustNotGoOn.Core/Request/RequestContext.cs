using ShowMustNotGoOn.Core.MessageBus;

namespace ShowMustNotGoOn.Core.Request
{
    public class RequestContext
    {
        public IMessage Message { get; }

        public RequestContext(IMessage message)
        {
            Message = message;
        }
    }
}
