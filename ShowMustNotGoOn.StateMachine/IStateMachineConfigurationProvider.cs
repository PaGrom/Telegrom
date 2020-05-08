using System;

namespace ShowMustNotGoOn.StateMachine
{
    public interface IStateMachineConfigurationProvider
    {
        Type InitialState { get; }
    }
}
