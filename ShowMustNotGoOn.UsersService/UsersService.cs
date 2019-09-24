using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Serilog;
using ShowMustNotGoOn.Core;
using ShowMustNotGoOn.Core.Model;
using ShowMustNotGoOn.DatabaseContext.Entities;

namespace ShowMustNotGoOn.UsersService
{
    public class UsersService : IUsersService
    {
        private readonly ShowsDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public UsersService(ShowsDbContext dbContext,
            IMapper mapper,
            ILogger logger)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<User> AddOrUpdateUserAsync(User user)
        {
            var newUser = _mapper.Map<Users>(user);
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            var existingUser = await _dbContext.Users.SingleOrDefaultAsync(u => u.TelegramId == user.TelegramId);
            if (existingUser != null)
            {
                _logger.Information($"User {user.Username} (Id: {user.TelegramId}) already exists in db");
                newUser = _mapper.Map(user, existingUser); // Update existing user
                _dbContext.Entry(existingUser).CurrentValues.SetValues(newUser);
            }
            else
            {
                _logger.Information($"Adding user {user.Username} (Id: {user.TelegramId}) to db");
                newUser = _dbContext.Users.Add(newUser).Entity;
            }

            await _dbContext.SaveChangesAsync();

            transaction.Commit();

            return _mapper.Map<User>(newUser);
        }
    }
}
