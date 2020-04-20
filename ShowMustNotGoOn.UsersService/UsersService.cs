using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Serilog;
using ShowMustNotGoOn.Core;
using ShowMustNotGoOn.Core.Model;

namespace ShowMustNotGoOn.UsersService
{
    public class UsersService : IUsersService
    {
        private readonly DbContextOptions _dbContextOptions;
        private readonly ILogger _logger;

        public UsersService(DbContextOptions dbContextOptions,
            ILogger logger)
        {
            _dbContextOptions = dbContextOptions;
            _logger = logger;
        }

        public async Task<User> AddOrUpdateUserAsync(User user, CancellationToken cancellationToken)
        {
	        await using var context = new DatabaseContext.DatabaseContext(_dbContextOptions);
            User newUser;

            var existingUser = await context.Users
                .Include(u => u.Subscriptions)
                .ThenInclude(s => s.TvShow)
                .SingleOrDefaultAsync(u => u.TelegramId == user.TelegramId, cancellationToken);
            if (existingUser != null)
            {
                _logger.Information($"User {user.Username} (Id: {user.TelegramId}) already exists in db");
                user.Id = existingUser.Id;
                context.Entry(existingUser).CurrentValues.SetValues(user);
                newUser = existingUser;
            }
            else
            {
                _logger.Information($"Adding user {user.Username} (Id: {user.TelegramId}) to db");
                newUser = (await context.Users.AddAsync(user, cancellationToken)).Entity;
            }

            await context.SaveChangesAsync(cancellationToken);

            return newUser;
        }

        public async Task SubscribeUserToTvShowAsync(User user, TvShow tvShow, SubscriptionType subscriptionType)
        {
	        await using var context = new DatabaseContext.DatabaseContext(_dbContextOptions);

            var savedShow = await context.TvShows
                .SingleOrDefaultAsync(s => s.MyShowsId == tvShow.MyShowsId);

            if (savedShow == null)
            {
                tvShow = context.TvShows.Add(tvShow).Entity;
                await context.SaveChangesAsync();
            }
            else
            {
                tvShow = savedShow;
            }

            var existedSubscription = user.Subscriptions
                .SingleOrDefault(s => s.SubscriptionType == subscriptionType
                                      && s.TvShow.MyShowsId == tvShow.MyShowsId);

            if (existedSubscription == null)
            {
                user.Subscriptions.Add(new Subscription
                {
                    User = user,
                    TvShow = tvShow,
                    SubscriptionType = subscriptionType
                });

                await context.SaveChangesAsync();
            }
        }

        public async Task UnsubscribeUserFromTvShowAsync(User user, TvShow tvShow, SubscriptionType subscriptionType)
        {
	        await using var context = new DatabaseContext.DatabaseContext(_dbContextOptions);

            var subscription = user.Subscriptions.SingleOrDefault(s => s.SubscriptionType == subscriptionType
                                                                       && s.TvShow.MyShowsId == tvShow.MyShowsId);
            if (subscription != null)
            {
                context.Subscriptions.Remove(subscription);
                await context.SaveChangesAsync();
            }
        }
    }
}
