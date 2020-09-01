using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Telegrom.Core;
using Telegrom.Database.Model;

namespace Telegrom.Database
{
    public sealed class GlobalAttributesService : IGlobalAttributesService, IAsyncDisposable
    {
        private readonly DatabaseContext _dbContext;

        public GlobalAttributesService(DbContextOptions dbContextOptions)
        {
            _dbContext = new DatabaseContext(dbContextOptions);
        }

        public IAsyncEnumerable<T> GetGlobalAttributesAsync<T>() where T : IGlobalAttribute
        {
            return _dbContext.GlobalAttributes
                .Where(ga => ga.Type.Equals(typeof(T).FullName))
                .Select(ga => JsonConvert.DeserializeObject<T>(ga.Value))
                .AsAsyncEnumerable();
        }

        public async Task<T> GetGlobalAttributeAsync<T>(Guid guid, CancellationToken cancellationToken) where T : IGlobalAttribute
        {
            var attribute = await _dbContext.GlobalAttributes
                .FindAsync(new object[] { guid, typeof(T).FullName }, cancellationToken);

            return JsonConvert.DeserializeObject<T>(attribute.Value);
        }

        public async Task CreateOrUpdateGlobalAttributeAsync<T>(T obj, CancellationToken cancellationToken) where T : IGlobalAttribute
        {
            var existedAttribute = await _dbContext.GlobalAttributes
                .FindAsync(new object[] { obj.Id, typeof(T).FullName }, cancellationToken);

            if (existedAttribute != null)
            {
                existedAttribute.Value = JsonConvert.SerializeObject(obj);
            }
            else
            {
                await _dbContext.GlobalAttributes.AddAsync(new GlobalAttribute
                {
                    Id = obj.Id,
                    Type = typeof(T).FullName,
                    Value = JsonConvert.SerializeObject(obj)
                }, cancellationToken);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public ValueTask DisposeAsync()
        {
            return _dbContext.DisposeAsync();
        }
    }
}
