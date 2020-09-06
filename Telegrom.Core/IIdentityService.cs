using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Telegrom.Core
{
    public interface IIdentityService
    {
        Task AddOrUpdateUserAsync(User user, CancellationToken cancellationToken);
    }
}
