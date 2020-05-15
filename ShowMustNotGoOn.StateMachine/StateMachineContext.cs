namespace ShowMustNotGoOn.StateMachine
{
    internal class StateMachineContext : IStateMachineContext
    {
        public StateMachineContext()
        {
            NextStateName = null;
        }

        public string NextStateName { get; private set; }

        public void MoveTo(string stateName)
        {
            NextStateName = stateName;
        }

        public void Reset()
        {
            NextStateName = null;
        }
    }
}
