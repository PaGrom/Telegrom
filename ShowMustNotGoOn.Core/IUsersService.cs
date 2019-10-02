using System.Threading.Tasks;
using ShowMustNotGoOn.Core.Model;

namespace ShowMustNotGoOn.Core
{
    public interface IUsersService
    {
        Task<User> AddOrUpdateUserAsync(User user);
        Task<bool> IsUserSubscribedToTvShowAsync(User user, TvShow tvShow, SubscriptionType type);
        Task<User> SubscribeUserToTvShowAsync(User user, TvShow tvShow, SubscriptionType subscriptionType);
    }
}