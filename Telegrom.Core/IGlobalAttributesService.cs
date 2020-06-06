using System;
using System.Threading;
using System.Threading.Tasks;

namespace Telegrom.Core
{
    public interface IGlobalAttributesService
    {
        Task<T> GetGlobalAttributeAsync<T>(Guid guid, CancellationToken cancellationToken);
        Task<Guid?> GetAttributeIdByValueAsync<T>(T value, CancellationToken cancellationToken);
        Task CreateOrUpdateGlobalAttributeAsync<T>(Guid guid, T obj, CancellationToken cancellationToken);
    }
}
