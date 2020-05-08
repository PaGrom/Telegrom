using ShowMustNotGoOn.StateMachine;

namespace ShowMustNotGoOn.States
{
    internal class SendWelcomeMessage : SendMessageAndThen<Finish>
    {
        public SendWelcomeMessage(IStateContext stateContext) : base(stateContext, "Привет!") { }
    }
}
