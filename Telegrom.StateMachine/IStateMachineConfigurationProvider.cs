namespace Telegrom.StateMachine
{
    public interface IStateMachineConfigurationProvider
    {
        string InitialStateName { get; }
        string DefaultStateName { get; }
    }
}
