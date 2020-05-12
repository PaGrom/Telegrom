using ShowMustNotGoOn.StateMachine;

namespace ShowMustNotGoOn.States
{
    internal class SendWelcomeMessage : SendMessageAndThen<HandleUpdate>
    {
        public SendWelcomeMessage(IStateContext stateContext) : base(stateContext, "Привет!") { }
    }
}
