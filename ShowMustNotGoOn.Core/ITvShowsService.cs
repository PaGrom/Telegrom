using System.Collections.Generic;
using System.Threading.Tasks;
using ShowMustNotGoOn.Core.Model;

namespace ShowMustNotGoOn.Core
{
    public interface ITvShowsService
    {
        Task<TvShow> AddNewTvShowAsync(TvShow tvShow);
        Task<IEnumerable<TvShow>> SearchTvShowsAsync(string name);
        Task<TvShow> GetTvShowFromMyShowsAsync(int myShowsId);
        Task<TvShow> GetTvShowAsync(int tvShowId);
        Task<TvShow> GetTvShowByMyShowsIdAsync(int myShowsId);
        Task<Subscription> GetUserSubscriptionToTvShowAsync(User user, TvShow show, SubscriptionType subscriptionType);
        Task<Subscription> SubscribeUserToTvShowAsync(User user, TvShow tvShow, SubscriptionType subscriptionType);
        Task UnsubscribeUserFromTvShowAsync(User user, TvShow tvShow, SubscriptionType subscriptionType);
        Task<List<Subscription>> GetUserSubscriptionsAsync(User user);
    }
}