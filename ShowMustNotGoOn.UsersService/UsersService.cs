using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShowMustNotGoOn.Core;
using ShowMustNotGoOn.Core.TelegramModel;
using ShowMustNotGoOn.DatabaseContext.Model;

namespace ShowMustNotGoOn.UsersService
{
    public class UsersService : IUsersService
    {
        private readonly DbContextOptions _dbContextOptions;
        private readonly ILogger<UsersService> _logger;

        public UsersService(DbContextOptions dbContextOptions,
            ILogger<UsersService> logger)
        {
            _dbContextOptions = dbContextOptions;
            _logger = logger;
        }

        public async Task<IdentityUser> AddOrUpdateUserAsync(User user, CancellationToken cancellationToken)
        {
            await using var context = new DatabaseContext.DatabaseContext(_dbContextOptions);

            var identityUser = await context.IdentityUsers
                .FindAsync(new object[] { user.Id }, cancellationToken);

            if (identityUser != null)
            {
                _logger.LogInformation($"IdentityUser {user.Username} (Id: {user.Id}) already exists in db");
            }
            else
            {
                _logger.LogInformation($"Adding identityUser {user.Username} (Id: {user.Id}) to db");
                identityUser = (await context.IdentityUsers.AddAsync(new IdentityUser
                {
                    Id = user.Id
                }, cancellationToken)).Entity;
            }

            identityUser.Username = user.Username;
            identityUser.FirstName = user.FirstName;
            identityUser.LastName = user.LastName;

            await context.SaveChangesAsync(cancellationToken);

            return identityUser;
        }
    }
}
