using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Telegrom.Core
{
    public interface IGlobalAttributesService
    {
        IAsyncEnumerable<T> GetGlobalAttributesAsync<T>() where T : IGlobalAttribute;
        Task<T> GetGlobalAttributeAsync<T>(Guid guid, CancellationToken cancellationToken) where T : IGlobalAttribute;
        Task CreateOrUpdateGlobalAttributeAsync<T>(T obj, CancellationToken cancellationToken) where T : IGlobalAttribute;
    }
}
