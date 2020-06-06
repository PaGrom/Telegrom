using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Telegrom.Core;
using Telegrom.Database.Model;

namespace Telegrom.Database
{
    public sealed class GlobalAttributesService : IGlobalAttributesService
    {
        private readonly DbContextOptions _dbContextOptions;

        public GlobalAttributesService(DbContextOptions dbContextOptions)
        {
            _dbContextOptions = dbContextOptions;
        }

        public async Task<T> GetGlobalAttributeAsync<T>(Guid guid, CancellationToken cancellationToken)
        {
            await using var context = new DatabaseContext(_dbContextOptions);
            var attribute = await context.GlobalAttributes
                .FindAsync(new object[] { guid, typeof(T).FullName }, cancellationToken);

            return JsonConvert.DeserializeObject<T>(attribute.Value);
        }

        public async Task<Guid?> GetAttributeIdByValueAsync<T>(T value, CancellationToken cancellationToken)
        {
            await using var context = new DatabaseContext(_dbContextOptions);
            var serializedValue = JsonConvert.SerializeObject(value);
            var attribute = await context.GlobalAttributes
                .Where(a => a.Type == typeof(T).FullName && a.Value == serializedValue)
                .FirstOrDefaultAsync(cancellationToken);
            
            return attribute?.Id;
        } 

        public async Task CreateOrUpdateGlobalAttributeAsync<T>(Guid guid, T obj, CancellationToken cancellationToken)
        {
            await using var context = new DatabaseContext(_dbContextOptions);

            var existedAttribute = await context.GlobalAttributes
                .FindAsync(new object[] { guid, typeof(T).FullName }, cancellationToken);

            if (existedAttribute != null)
            {
                existedAttribute.Value = JsonConvert.SerializeObject(obj);
            }
            else
            {
                await context.GlobalAttributes.AddAsync(new GlobalAttribute
                {
                    Id = guid,
                    Type = typeof(T).FullName,
                    Value = JsonConvert.SerializeObject(obj)
                }, cancellationToken);
            }

            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
