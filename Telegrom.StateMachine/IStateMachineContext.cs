namespace Telegrom.StateMachine
{
    public interface IStateMachineContext
    {
        string NextStateName { get; }
        void MoveTo(string stateName);
        void Reset();
    }
}
