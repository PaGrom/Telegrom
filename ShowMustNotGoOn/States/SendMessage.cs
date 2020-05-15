using System.Threading;
using System.Threading.Tasks;
using ShowMustNotGoOn.Core.TelegramModel;
using ShowMustNotGoOn.StateMachine;

namespace ShowMustNotGoOn.States
{
    public abstract class SendMessage : IState
    {
        private readonly IStateContext _stateContext;
        private readonly string _message;

        protected SendMessage(IStateContext stateContext, string message)
        {
            _stateContext = stateContext;
            _message = message;
        }

        public async Task OnEnter(CancellationToken cancellationToken)
        {
            var request = new SendMessageRequest(_stateContext.UpdateContext.SessionContext.User.Id, _message);
            await _stateContext.UpdateContext.SessionContext.PostRequestAsync(request, cancellationToken);
        }
    }
}
