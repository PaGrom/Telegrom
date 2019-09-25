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
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
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
    }
}
