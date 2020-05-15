using System;
using ShowMustNotGoOn.StateMachine;

namespace ShowMustNotGoOn.States
{
    internal class StateMachineConfigurationProvider : IStateMachineConfigurationProvider
    {
        public string InitialStateName { get; }

        public StateMachineConfigurationProvider(string initialStateName)
        {
            InitialStateName = initialStateName;
        }
    }
}
