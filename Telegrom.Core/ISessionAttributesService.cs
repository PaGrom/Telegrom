using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Telegrom.Core
{
    public interface ISessionAttributesService
    {
        Task<T> GetSessionAttributeAsync<T>(Guid guid, CancellationToken cancellationToken);
        Task<IEnumerable<T>> GetAllByTypeAsync<T>(CancellationToken cancellationToken);
        Task SaveOrUpdateSessionAttributeAsync<T>(Guid guid, T obj, CancellationToken cancellationToken);
    }
}
