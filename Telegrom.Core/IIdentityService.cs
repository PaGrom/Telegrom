using System.Threading;
using System.Threading.Tasks;
using Telegrom.Core.TelegramModel;

namespace Telegrom.Core
{
    public interface IIdentityService
    {
        Task AddOrUpdateUserAsync(User user, CancellationToken cancellationToken);
    }
}
