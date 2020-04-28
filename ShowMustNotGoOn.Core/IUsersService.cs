using System.Threading;
using System.Threading.Tasks;
using ShowMustNotGoOn.DatabaseContext.Model;

namespace ShowMustNotGoOn.Core
{
    public interface IUsersService
    {
        Task<User> AddOrUpdateUserAsync(User user, CancellationToken cancellationToken);
    }
}