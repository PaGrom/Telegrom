using System;
using System.Collections.Generic;

namespace ShowMustNotGoOn.StateMachine.Builder
{
    public interface IStateNode
    {
        IStateNode AddNext<T>(NextStateType nextStateType) where T : IState;
        IStateNode AddNext(Type nextState, NextStateType nextStateType);
        IStateNode AddNext(IStateNode stateNode, NextStateType nextStateType);
        ICollection<IStateNode> AddNext(params ConditionalNextState[] conditionalNextStates);
    }
}
