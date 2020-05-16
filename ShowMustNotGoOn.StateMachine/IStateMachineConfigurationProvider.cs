using System;

namespace ShowMustNotGoOn.StateMachine
{
    public interface IStateMachineConfigurationProvider
    {
        string InitialStateName { get; }
        string DefaultStateName { get; }
    }
}
