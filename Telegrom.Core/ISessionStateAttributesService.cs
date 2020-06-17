using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Telegrom.Core
{
    public interface ISessionStateAttributesService
    {
        Task CreateOrUpdateStateAttributeAsync(string name, Type type, object obj, CancellationToken cancellationToken);
        IAsyncEnumerable<(string name, Type type, object obj)> GetAllStateAttributesAsync(CancellationToken cancellationToken);
        Task RemoveAllStateAttributes(CancellationToken cancellationToken);
    }
}
