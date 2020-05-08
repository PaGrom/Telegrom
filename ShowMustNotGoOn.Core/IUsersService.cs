using System.Threading;
using System.Threading.Tasks;
using ShowMustNotGoOn.Core.TelegramModel;
using ShowMustNotGoOn.DatabaseContext.Model;

namespace ShowMustNotGoOn.Core
{
    public interface IUsersService
    {
        Task<IdentityUser> AddOrUpdateUserAsync(User user, CancellationToken cancellationToken);
        Task<string> GetOrSetDefaultCurrentStateAsync(User user, string defaultStateName, CancellationToken cancellationToken);
        Task UpdateCurrentStateAsync(User user, string stateName, CancellationToken cancellationToken);
    }
}
