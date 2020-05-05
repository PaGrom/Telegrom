using ShowMustNotGoOn.Core.Session;
using ShowMustNotGoOn.Core.TelegramModel;

namespace ShowMustNotGoOn.Core.Request
{
    public class RequestContext
    {
        public SessionContext SessionContext { get; }
        public Update Update { get; }

        public RequestContext(SessionContext sessionContext, Update update)
        {
            SessionContext = sessionContext;
            Update = update;
        }
    }
}
