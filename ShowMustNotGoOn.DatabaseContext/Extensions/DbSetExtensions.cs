using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ShowMustNotGoOn.DatabaseContext.Extensions
{
    public static class DbSetExtensions
    {
        public static async Task<T> AddIfNotExistsAsync<T>(this DbSet<T> dbSet,
            T entity,
            Expression<Func<T, bool>> predicate = null,
            CancellationToken cancellationToken = default) where T : class, new()
        {
            var existed = predicate != null
                ? await dbSet.FirstOrDefaultAsync(predicate, cancellationToken)
                : await dbSet.FirstOrDefaultAsync(cancellationToken);

            return existed ?? (await dbSet.AddAsync(entity, cancellationToken)).Entity;
        }
    }
}
