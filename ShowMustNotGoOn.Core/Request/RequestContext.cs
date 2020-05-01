using Telegram.Bot.Types;

namespace ShowMustNotGoOn.Core.Request
{
    public class RequestContext
    {
        public Update Update { get; }

        public RequestContext(Update update)
        {
            Update = update;
        }
    }
}
