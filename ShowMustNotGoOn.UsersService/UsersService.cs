using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Serilog;
using ShowMustNotGoOn.Core;
using ShowMustNotGoOn.Core.Model;

namespace ShowMustNotGoOn.UsersService
{
    public class UsersService : IUsersService
    {
        private readonly DatabaseContext.DatabaseContext _dbContext;
        private readonly ILogger _logger;

        public UsersService(DatabaseContext.DatabaseContext dbContext,
            ILogger logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<User> AddOrUpdateUserAsync(User user)
        {
            User newUser;
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                    var existingUser = await _dbContext.Users
                    .Include(u => u.Subscriptions)
                    .ThenInclude(s => s.TvShow)
                    .SingleOrDefaultAsync(u => u.TelegramId == user.TelegramId);
                if (existingUser != null)
                {
                    _logger.Information($"User {user.Username} (Id: {user.TelegramId}) already exists in db");
                    user.Id = existingUser.Id;
                    _dbContext.Entry(existingUser).CurrentValues.SetValues(user);
                    newUser = existingUser;
                }
                else
                {
                    _logger.Information($"Adding user {user.Username} (Id: {user.TelegramId}) to db");
                    newUser = _dbContext.Users.Add(user).Entity;
                }

                await _dbContext.SaveChangesAsync();

                transaction.Commit();

                return newUser;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error while add or update user");
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task SubscribeUserToTvShowAsync(User user, TvShow tvShow, SubscriptionType subscriptionType)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var savedShow = await _dbContext.TvShows
                    .SingleOrDefaultAsync(s => s.MyShowsId == tvShow.MyShowsId);

                if (savedShow == null)
                {
                    tvShow = _dbContext.TvShows.Add(tvShow).Entity;
                    await _dbContext.SaveChangesAsync();
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

                    await _dbContext.SaveChangesAsync();
                }

                await transaction.CommitAsync();
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error while subscribe user");
                await transaction.RollbackAsync();
            }
        }

        public async Task UnsubscribeUserFromTvShowAsync(User user, TvShow tvShow, SubscriptionType subscriptionType)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var subscription = user.Subscriptions.SingleOrDefault(s => s.SubscriptionType == subscriptionType
                                                                           && s.TvShow.MyShowsId == tvShow.MyShowsId);
                if (subscription != null)
                {
                    _dbContext.Subscriptions.Remove(subscription);
                    await _dbContext.SaveChangesAsync();
                }
                
                await transaction.CommitAsync();
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error while unsubscribe user");
                await transaction.RollbackAsync();
            }
        }
    }
}
