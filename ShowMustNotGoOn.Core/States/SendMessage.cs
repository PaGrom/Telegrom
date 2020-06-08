using System.Threading;
using System.Threading.Tasks;
using Telegrom.Core.TelegramModel;
using Telegrom.StateMachine;

namespace ShowMustNotGoOn.Core.States
{
    public abstract class SendMessage : StateBase
    {
        private readonly IStateContext _stateContext;
        private readonly string _message;

        protected SendMessage(IStateContext stateContext, string message)
        {
            _stateContext = stateContext;
            _message = message;
        }

        public override Task OnEnter(CancellationToken cancellationToken)
        {
            var request = new SendMessageRequest(_stateContext.UpdateContext.SessionContext.User.Id, _message);
            return _stateContext.UpdateContext.SessionContext.PostRequestAsync(request, cancellationToken);
        }
    }
}
