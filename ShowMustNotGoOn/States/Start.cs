using ShowMustNotGoOn.StateMachine;

namespace ShowMustNotGoOn.States
{
    internal class Start : WaitForStartCommandAndThen<SendWelcomeMessage>
    {
        public Start(IStateContext stateContext) : base(stateContext) { }
    }
}
