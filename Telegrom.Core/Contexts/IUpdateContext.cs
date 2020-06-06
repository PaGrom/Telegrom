using Telegrom.Core.TelegramModel;

namespace Telegrom.Core.Contexts
{
    public interface IUpdateContext
    {
        SessionContext SessionContext { get; }
        Update Update { get; }
    }
}
