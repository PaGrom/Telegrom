using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Telegrom.Core;
using Telegrom.Database;
using Telegrom.Database.Model;

namespace Telegrom
{
    public sealed class TelegromClient
    {
        private readonly DatabaseContext _databaseContext;

        public TelegromClient()
        {
            var optionsBuilder = DatabaseOptions.Current ?? throw new Exception("You have to configure db");
            var options = optionsBuilder
                .EnableSensitiveDataLogging()
                .Options;
            _databaseContext = new DatabaseContext(options);
        }
        public IAsyncEnumerable<IdentityUser> GetUsers()
        {
            return _databaseContext.IdentityUsers.AsAsyncEnumerable();
        }

        public IAsyncEnumerable<T> GetUserAttributes<T>(IdentityUser user) where T : ISessionAttribute
        {
            return _databaseContext.SessionAttributes
                .Where(a => a.SessionId == user.Id && a.Type == typeof(T).FullName)
                .Select(a => JsonConvert.DeserializeObject<T>(a.Value))
                .AsAsyncEnumerable();
        }

        public IAsyncEnumerable<T> GetGlobalAttributes<T>()
        {
            return _databaseContext.GlobalAttributes
                .Where(ga => ga.Type.Equals(typeof(T).FullName))
                .Select(ga => JsonConvert.DeserializeObject<T>(ga.Value))
                .AsAsyncEnumerable();
        }
    }
}
