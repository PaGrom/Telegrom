using System.Threading;
using System.Threading.Tasks;
using ShowMustNotGoOn.Core.TelegramModel;
using ShowMustNotGoOn.StateMachine;
using ShowMustNotGoOn.StateMachine.Attributes;

namespace ShowMustNotGoOn.States
{
    internal sealed class GenerateKeyboard : StateBase
    {
        private readonly IStateContext _stateContext;

        [Output]
        public InlineKeyboardMarkup InlineKeyboardMarkup { get; set; }

        public GenerateKeyboard(IStateContext stateContext)
        {
            _stateContext = stateContext;
        }

        public override Task OnEnter(CancellationToken cancellationToken)
        {
            InlineKeyboardMarkup = InlineKeyboardMarkup.Empty();

            return Task.CompletedTask;
        }
    }
}
