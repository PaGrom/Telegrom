using ShowMustNotGoOn.Core.TelegramModel;

namespace ShowMustNotGoOn.Core.Contexts
{
    public interface IUpdateContext
    {
        SessionContext SessionContext { get; }
        Update Update { get; }
    }
}
