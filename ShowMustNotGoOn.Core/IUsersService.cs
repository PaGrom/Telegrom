using System.Threading;
using System.Threading.Tasks;
using ShowMustNotGoOn.DatabaseContext.Model;
using Telegram.Bot.Types;

namespace ShowMustNotGoOn.Core
{
    public interface IUsersService
    {
        Task<IdentityUser> AddOrUpdateUserAsync(User user, CancellationToken cancellationToken);
    }
}
