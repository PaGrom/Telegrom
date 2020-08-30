using Telegram.Bot.Types;

namespace Telegrom.Core.Contexts
{
    public interface IUpdateContext
    {
        SessionContext SessionContext { get; }
        Update Update { get; }
    }
}
