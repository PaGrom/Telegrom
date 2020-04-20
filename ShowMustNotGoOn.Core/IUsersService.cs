﻿using System.Threading;
using System.Threading.Tasks;
using ShowMustNotGoOn.Core.Model;

namespace ShowMustNotGoOn.Core
{
    public interface IUsersService
    {
        Task<User> AddOrUpdateUserAsync(User user, CancellationToken cancellationToken);
        Task SubscribeUserToTvShowAsync(User user, TvShow tvShow, SubscriptionType subscriptionType);
        Task UnsubscribeUserFromTvShowAsync(User user, TvShow tvShow, SubscriptionType subscriptionType);
    }
}