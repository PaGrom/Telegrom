﻿namespace Telegrom.StateMachine
{
    public class StateMachineConfigurationProvider : IStateMachineConfigurationProvider
    {
        public string InitialStateName { get; }
        public string DefaultStateName { get; }

        public StateMachineConfigurationProvider(string initialStateName, string defaultStateName)
        {
            InitialStateName = initialStateName;
            DefaultStateName = defaultStateName;
        }
    }
}
