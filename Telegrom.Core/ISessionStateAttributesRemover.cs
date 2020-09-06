using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Telegrom.Core
{
    public interface ISessionStateAttributesRemover
    {
        Task RemoveAllSessionStateAttributesAsync(User user, CancellationToken cancellationToken);
    }
}
