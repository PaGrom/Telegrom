using System.Threading;
using System.Threading.Tasks;

namespace ShowMustNotGoOn.Core
{
    public interface IUpdateHandler
    {
        Task Execute(CancellationToken cancellationToken);
    }
}
