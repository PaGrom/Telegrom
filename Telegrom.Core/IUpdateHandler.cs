using System.Threading;
using System.Threading.Tasks;

namespace Telegrom.Core
{
    public interface IUpdateHandler
    {
        Task Execute(CancellationToken cancellationToken);
    }
}
