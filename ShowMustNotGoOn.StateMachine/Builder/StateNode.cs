using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using ShowMustNotGoOn.Core.Extensions;

namespace ShowMustNotGoOn.StateMachine.Builder
{
    internal sealed class StateNode: IStateNode
    {
        public Type StateType { get; }

        public string GeneratedTypeName { get; private set; }

        public List<ConditionalNextStateNode> ConditionalNextStateNodes = new List<ConditionalNextStateNode>();

        public StateNode(Type stateType)
        {
            StateType = stateType;
        }

        public IStateNode AddNext<T>(NextStateType nextStateType) where T : IState
        {
            return AddNext(new ConditionalNextState(typeof(T), nextStateType, context => Task.FromResult(true))).Single();
        }

        public IStateNode AddNext(Type nextState, NextStateType nextStateType)
        {
            if (!typeof(IState).IsAssignableFrom(nextState))
            {
                throw new ArgumentException(nameof(nextState), $"Type must implement interface {nameof(IState)}");
            }

            return AddNext(new ConditionalNextState(nextState, nextStateType, context => Task.FromResult(true))).Single();
        }

        public IStateNode AddNext(IStateNode stateNode, NextStateType nextStateType)
        {
            return AddNext(new ConditionalNextState(stateNode, nextStateType, context => Task.FromResult(true))).Single();
        }

        public ICollection<IStateNode> AddNext(params ConditionalNextState[] conditionalNextStates)
        {
            if (conditionalNextStates.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(conditionalNextStates),
                    "There must be at least one conditional next state");
            }

            foreach (var conditionalNextState in conditionalNextStates)
            {
                StateNode nextStateNode = null;
                if (conditionalNextState.NextState != null)
                {
                    nextStateNode = new StateNode(conditionalNextState.NextState);
                }

                if (conditionalNextState.NextStateNode != null)
                {
                    nextStateNode = (StateNode)conditionalNextState.NextStateNode;
                }

                var conditionalNextStateNode = new ConditionalNextStateNode
                {
                    NextStateNode = nextStateNode,
                    NextStateType = conditionalNextState.NextStateType,
                    Condition = conditionalNextState.Condition
                };

                ConditionalNextStateNodes.Add(conditionalNextStateNode);
            }

            return ConditionalNextStateNodes.Select(n => (IStateNode)n.NextStateNode).ToList();
        }

        // TODO: If/else is enough
        //public (IStateNode IfTrueState, IStateNode ElseState) AddNext(NextStateType nextStateType, Func<IStateContext, Task<bool>> condition, Type ifTrue, Type @else)
        //{
        //    if (!typeof(IState).IsAssignableFrom(ifTrue))
        //    {
        //        throw new ArgumentException(nameof(ifTrue), $"Type must implement interface {nameof(IState)}");
        //    }

        //    if (!typeof(IState).IsAssignableFrom(@else))
        //    {
        //        throw new ArgumentException(nameof(@else), $"Type must implement interface {nameof(IState)}");
        //    }

        //    ConditionalStates = (condition, new StateNode(ifTrue), new StateNode(@else));
        //    return (ConditionalStates.IfTrueStateNode, ConditionalStates.ElseStateNode);
        //}

        //public (IStateNode IfTrueState, IStateNode ElseState) AddNext(NextStateType nextStateType, Func<IStateContext, Task<bool>> condition, Type ifTrue, IStateNode elseStateNode)
        //{
        //    if (!typeof(IState).IsAssignableFrom(ifTrue))
        //    {
        //        throw new ArgumentException(nameof(ifTrue), $"Type must implement interface {nameof(IState)}");
        //    }

        //    ConditionalStates = (condition, new StateNode(ifTrue), (StateNode)elseStateNode);
        //    return (ConditionalStates.IfTrueStateNode, ConditionalStates.ElseStateNode);
        //}

        //public (IStateNode IfTrueState, IStateNode ElseState) AddNext(NextStateType nextStateType, Func<IStateContext, Task<bool>> condition, IStateNode ifTrueStateNode, Type @else)
        //{
        //    if (!typeof(IState).IsAssignableFrom(@else))
        //    {
        //        throw new ArgumentException(nameof(@else), $"Type must implement interface {nameof(IState)}");
        //    }

        //    ConditionalStates = (condition, (StateNode)ifTrueStateNode, new StateNode(@else));
        //    return (ConditionalStates.IfTrueStateNode, ConditionalStates.ElseStateNode);
        //}

        //public (IStateNode IfTrueState, IStateNode ElseState) AddNext(Func<IStateContext, Task<bool>> condition, IStateNode ifTrueStateNode, IStateNode elseStateNode)
        //{
        //    ConditionalStates = (condition, (StateNode)ifTrueStateNode, (StateNode)elseStateNode);
        //    return (ConditionalStates.IfTrueStateNode, ConditionalStates.ElseStateNode);
        //}

        //public (IStateNode IfTrueState, IStateNode ElseState) AddNext<TIfTrue, TElse>(Func<IStateContext, Task<bool>> condition)
        //    where TIfTrue : IState
        //    where TElse : IState
        //{
        //    ConditionalStates = (condition, new StateNode(typeof(TIfTrue)), new StateNode(typeof(TElse)));
        //    return (ConditionalStates.IfTrueStateNode, ConditionalStates.ElseStateNode);
        //}

        public void Register(ContainerBuilder containerBuilder)
        {
            if (GeneratedTypeName != null)
            {
                return;
            }

            GeneratedTypeName = $"{nameof(GeneratedState)}<{StateType.Name}->{string.Join(';', ConditionalNextStateNodes.Select(n => $"{n.NextStateType}:{n.NextStateNode.StateType.Name}"))}>";

            containerBuilder.RegisterType<GeneratedState>()
                .WithParameter(
                    new ResolvedParameter(
                        (pi, ctx) => pi.ParameterType.IsAssignableFrom(typeof(IState)),
                        (pi, ctx) => ctx.ResolveNamed<IState>(StateType.Name)))
                .WithParameter(new TypedParameter(typeof(StateNode), this))
                .Named<IState>(GeneratedTypeName)
                .InstancePerUpdate();

            ConditionalNextStateNodes.ForEach(n => n.NextStateNode.Register(containerBuilder));
        }
    }
}
