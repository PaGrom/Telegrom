using System.Threading;
using System.Threading.Tasks;
using Telegrom.Core.TelegramModel;

namespace Telegrom.Core
{
    public interface ISessionStateAttributesRemover
    {
        Task RemoveAllSessionStateAttributesAsync(User user, CancellationToken cancellationToken);
    }
}
