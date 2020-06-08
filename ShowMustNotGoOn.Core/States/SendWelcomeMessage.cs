using Telegrom.StateMachine;

namespace ShowMustNotGoOn.Core.States
{
    public class SendWelcomeMessage : SendMessage
    {
        public SendWelcomeMessage(IStateContext stateContext) : base(stateContext, "Привет!") { }
    }
}
