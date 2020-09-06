﻿using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegrom.Core;
using Telegrom.Database.Model;

namespace Telegrom.Database
{
    public class IdentityService : IIdentityService
    {
        private readonly DbContextOptions _dbContextOptions;
        private readonly ILogger<IdentityService> _logger;

        public IdentityService(DbContextOptions dbContextOptions,
            ILogger<IdentityService> logger)
        {
            _dbContextOptions = dbContextOptions;
            _logger = logger;
        }

        public async Task AddOrUpdateUserAsync(User user, CancellationToken cancellationToken)
        {
            await using var context = new DatabaseContext(_dbContextOptions);

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
        }
    }
}
