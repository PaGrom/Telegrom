using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Telegrom.Core;
using Telegrom.Core.Contexts;
using Telegrom.Database.Model;

namespace Telegrom.Database
{
    public sealed class SessionAttributesService : ISessionAttributesService
    {
        private readonly DatabaseContext _databaseContext;
        private readonly SessionContext _sessionContext;

        public SessionAttributesService(DatabaseContext databaseContext, SessionContext sessionContext)
        {
            _databaseContext = databaseContext;
            _sessionContext = sessionContext;
        }

        public async Task<T> GetSessionAttributeAsync<T>(Guid guid, CancellationToken cancellationToken) where T: ISessionAttribute
        {
            var attribute = await _databaseContext.SessionAttributes
                .FindAsync(new object[] { guid, _sessionContext.User.Id, typeof(T).FullName }, cancellationToken);

            return JsonConvert.DeserializeObject<T>(attribute.Value);
        }

        public async Task<IEnumerable<T>> GetAllByTypeAsync<T>(CancellationToken cancellationToken) where T : ISessionAttribute
        {
            var values = await _databaseContext.SessionAttributes
                .Where(a => a.SessionId == _sessionContext.User.Id && a.Type == typeof(T).FullName)
                .Select(a => JsonConvert.DeserializeObject<T>(a.Value))
                .ToListAsync(cancellationToken);

            return values;
        }

        public async Task SaveOrUpdateSessionAttributeAsync<T>(T obj, CancellationToken cancellationToken) where T : ISessionAttribute
        {
            var existedAttribute = await _databaseContext.SessionAttributes
                .FindAsync(new object[] { obj.Id, _sessionContext.User.Id, typeof(T).FullName }, cancellationToken);

            if (existedAttribute != null)
            {
                existedAttribute.Value = JsonConvert.SerializeObject(obj);
            }
            else
            {
                await _databaseContext.SessionAttributes.AddAsync(new SessionAttribute
                {
                    Id = obj.Id,
                    SessionId = _sessionContext.User.Id,
                    Type = typeof(T).FullName,
                    Value = JsonConvert.SerializeObject(obj)
                }, cancellationToken);
            }

            await _databaseContext.SaveChangesAsync(cancellationToken);
        }

        public async Task RemoveSessionAttributeAsync<T>(T obj, CancellationToken cancellationToken) where T : ISessionAttribute
        {
            var attribute = await _databaseContext.SessionAttributes
                .FindAsync(new object[] {obj.Id, _sessionContext.User.Id, typeof(T).FullName}, cancellationToken);

            _databaseContext.SessionAttributes.Remove(attribute);

            await _databaseContext.SaveChangesAsync(cancellationToken);
        }
    }
}
