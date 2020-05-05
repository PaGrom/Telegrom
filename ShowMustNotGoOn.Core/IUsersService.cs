using System.Threading;
using System.Threading.Tasks;
using ShowMustNotGoOn.Core.TelegramModel;
using ShowMustNotGoOn.DatabaseContext.Model;

namespace ShowMustNotGoOn.Core
{
    public interface IUsersService
    {
        Task<IdentityUser> AddOrUpdateUserAsync(User user, CancellationToken cancellationToken);
    }
}
