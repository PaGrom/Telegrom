using System;
using System.Threading;
using System.Threading.Tasks;
using ShowMustNotGoOn.Core.TelegramModel;
using ShowMustNotGoOn.StateMachine;

namespace ShowMustNotGoOn.States
{
    public abstract class WaitForStartCommandAndThen<TNext> : IState
        where TNext : IState
    {
        private const string Command = "/start";

        private readonly IStateContext _stateContext;

        protected WaitForStartCommandAndThen(IStateContext stateContext)
        {
            _stateContext = stateContext;
        }

        public Task Handle(CancellationToken cancellationToken)
        {
            if (!(_stateContext.UpdateContext.Update is Message message))
            {
                return Task.CompletedTask;
            }

            var text = message.Text.Trim();

            if (!string.Equals(text, Command, StringComparison.InvariantCultureIgnoreCase))
            {
                return Task.CompletedTask;
            }

            _stateContext.StateMachineContext.MoveTo<TNext>();
            return Task.CompletedTask;
        }
    }
}
