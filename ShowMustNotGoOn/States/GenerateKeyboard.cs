using System.Threading;
using System.Threading.Tasks;
using Telegrom.Core.TelegramModel;
using Telegrom.StateMachine;
using Telegrom.StateMachine.Attributes;

namespace ShowMustNotGoOn.States
{
    internal sealed class GenerateKeyboard : StateBase
    {
        [Output]
        public InlineKeyboardMarkup InlineKeyboardMarkup { get; set; }

        public override Task OnEnter(CancellationToken cancellationToken)
        {
            InlineKeyboardMarkup = InlineKeyboardMarkup.Empty();

            return Task.CompletedTask;
        }
    }
}
