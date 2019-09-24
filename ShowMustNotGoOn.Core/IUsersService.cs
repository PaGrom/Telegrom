using System.Threading.Tasks;
using ShowMustNotGoOn.Core.Model;

namespace ShowMustNotGoOn.Core
{
    public interface IUsersService
    {
        Task<User> AddOrUpdateUserAsync(User user);
    }
}