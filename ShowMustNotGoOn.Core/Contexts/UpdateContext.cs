using ShowMustNotGoOn.Core.TelegramModel;

namespace ShowMustNotGoOn.Core.Contexts
{
    public class UpdateContext : IUpdateContext
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
