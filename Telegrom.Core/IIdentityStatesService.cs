using System.Threading;
using System.Threading.Tasks;

namespace Telegrom.Core
{
    public interface IIdentityStatesService
    {
        Task<string> GetOrSetDefaultCurrentStateAsync(string defaultStateName, CancellationToken cancellationToken);
        Task UpdateCurrentStateAsync(string stateName, CancellationToken cancellationToken);
    }
}
