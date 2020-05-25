using System.Threading;
using System.Threading.Tasks;
using ShowMustNotGoOn.Core.TelegramModel;
using ShowMustNotGoOn.StateMachine;
using ShowMustNotGoOn.StateMachine.Attributes;

namespace ShowMustNotGoOn.States
{
    internal class SendSendPhotoRequest : StateBase
    {
        private readonly IStateContext _stateContext;

        [Input]
        public SendPhotoRequest SendPhotoRequest { get; set; }

        [Input]
        public InlineKeyboardMarkup InlineKeyboardMarkup { get; set; }

        public SendSendPhotoRequest(IStateContext stateContext)
        {
            _stateContext = stateContext;
        }

        public override async Task OnEnter(CancellationToken cancellationToken)
        {
            SendPhotoRequest.ReplyMarkup = InlineKeyboardMarkup;
            await _stateContext.UpdateContext.SessionContext.PostRequestAsync(SendPhotoRequest, cancellationToken);
        }
    }
}
