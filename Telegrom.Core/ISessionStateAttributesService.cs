using System.Threading;
using System.Threading.Tasks;

namespace Telegrom.Core
{
    public interface ISessionStateAttributesService
    {
        Task<string> GetOrSetDefaultCurrentStateAsync(string defaultStateName, CancellationToken cancellationToken);
        Task UpdateCurrentStateAsync(string stateName, CancellationToken cancellationToken);
    }
}
