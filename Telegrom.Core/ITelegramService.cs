using System.Threading;
using System.Threading.Tasks;
using Telegrom.Core.TelegramModel;

namespace Telegrom.Core
{
    public interface ITelegramService
    {
	    Task MakeRequestAsync(RequestBase requestBase, CancellationToken cancellationToken);
    }
}
