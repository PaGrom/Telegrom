using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Telegrom.Core
{
    public interface ISessionAttributesService
    {
        Task<T> GetSessionAttributeAsync<T>(Guid guid, CancellationToken cancellationToken) where T : ISessionAttribute;
        Task<IEnumerable<T>> GetAllByTypeAsync<T>(CancellationToken cancellationToken) where T : ISessionAttribute;
        Task SaveOrUpdateSessionAttributeAsync<T>(T obj, CancellationToken cancellationToken) where T : ISessionAttribute;
        Task RemoveSessionAttributeAsync<T>(T obj, CancellationToken cancellationToken) where T : ISessionAttribute;
        Task<IEnumerable<T>> FindAttributesInAllSessionsAsync<T>(Func<T, bool> predicate, CancellationToken cancellationToken) where T : ISessionAttribute;
    }
}
