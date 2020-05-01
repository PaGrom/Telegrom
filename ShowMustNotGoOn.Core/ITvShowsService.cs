using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ShowMustNotGoOn.DatabaseContext.Model;
using Telegram.Bot.Types;

namespace ShowMustNotGoOn.Core
{
    public interface ITvShowsService
    {
        Task<TvShow> AddNewTvShowAsync(TvShow tvShow, CancellationToken cancellationToken);
        Task<IEnumerable<TvShow>> SearchTvShowsAsync(string name, CancellationToken cancellationToken);
        Task<TvShow> GetTvShowFromMyShowsAsync(int myShowsId, CancellationToken cancellationToken);
        Task<TvShow> GetTvShowAsync(int tvShowId, CancellationToken cancellationToken);
        Task<TvShow> GetTvShowByMyShowsIdAsync(int myShowsId, CancellationToken cancellationToken);
        Task<Subscription> GetUserSubscriptionToTvShowAsync(User user, TvShow show, SubscriptionType subscriptionType, CancellationToken cancellationToken);
        Task<Subscription> SubscribeUserToTvShowAsync(User user, TvShow tvShow, SubscriptionType subscriptionType, CancellationToken cancellationToken);
        Task UnsubscribeUserFromTvShowAsync(User user, TvShow tvShow, SubscriptionType subscriptionType, CancellationToken cancellationToken);
        Task<List<Subscription>> GetUserSubscriptionsAsync(User user, CancellationToken cancellationToken);
    }
}
