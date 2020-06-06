using Telegrom.StateMachine;

namespace ShowMustNotGoOn.States
{
    internal class SendWelcomeMessage : SendMessage
    {
        public SendWelcomeMessage(IStateContext stateContext) : base(stateContext, "Привет!") { }
    }
}
