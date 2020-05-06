using ShowMustNotGoOn.Core.TelegramModel;

namespace ShowMustNotGoOn.Core.Contexts
{
    public class UpdateContext
    {
        public SessionContext SessionContext { get; }
        public Update Update { get; }

        public UpdateContext(SessionContext sessionContext, Update update)
        {
            SessionContext = sessionContext;
            Update = update;
        }
    }
}
