using ShowMustNotGoOn.DatabaseContext.Model;

namespace ShowMustNotGoOn.Core.MessageBus
{
    public interface IMessage
    {
        User User { get; }
    }
}
