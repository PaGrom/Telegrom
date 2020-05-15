using System;
using System.Threading;
using System.Threading.Tasks;
using ShowMustNotGoOn.Core.TelegramModel;
using ShowMustNotGoOn.StateMachine;

namespace ShowMustNotGoOn.States
{
    internal class Start : IState
    {
        private const string Command = "/start";

        private readonly IStateContext _stateContext;

        public Start(IStateContext stateContext)
        {
            _stateContext = stateContext;
        }

        public Task<bool> Handle(CancellationToken cancellationToken)
        {
            if (!(_stateContext.UpdateContext.Update is Message message))
            {
                return Task.FromResult(false);
            }

            var text = message.Text.Trim();

            if (!string.Equals(text, Command, StringComparison.InvariantCultureIgnoreCase))
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }
    }
}
