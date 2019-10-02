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
            var existingUser = await _dbContext.Users.SingleOrDefaultAsync(u => u.TelegramId == user.TelegramId);
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

        public async Task<bool> IsUserSubscribedToTvShowAsync(User user, TvShow tvShow, SubscriptionType type)
        {
            var registeredUser = await _dbContext.Users
                .Include(u => u.Subscriptions)
                .SingleOrDefaultAsync(u => u.TelegramId == user.TelegramId);

            if (registeredUser != null)
            {
                return registeredUser.Subscriptions.Any(s => s.TvShow.MyShowsId == tvShow.MyShowsId
                                                             && s.SubscriptionType == type);
            }

            var message = $"Can't find user with Id {user.TelegramId}";
            _logger.Error(message);
            throw new ArgumentOutOfRangeException();
        }

        public async Task<User> SubscribeUserToTvShowAsync(User user, TvShow tvShow, SubscriptionType subscriptionType)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            user = await _dbContext.Users.SingleAsync(u => u.TelegramId == user.TelegramId);
            tvShow = await _dbContext.TvShows.SingleAsync(s => s.MyShowsId == tvShow.MyShowsId);
            user.Subscriptions.Add(new Subscription
            {
                User = user,
                TvShow = tvShow,
                SubscriptionType = subscriptionType
            });
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            return user;
        }
    }
}
