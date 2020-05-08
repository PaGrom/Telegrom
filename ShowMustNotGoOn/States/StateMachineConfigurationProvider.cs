using System;
using ShowMustNotGoOn.StateMachine;

namespace ShowMustNotGoOn.States
{
    internal class StateMachineConfigurationProvider : IStateMachineConfigurationProvider
    {
        public Type InitialState => typeof(Start);
    }
}
