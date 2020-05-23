using System;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using ShowMustNotGoOn.Core.Extensions;

namespace ShowMustNotGoOn.StateMachine.Builder
{
    public sealed class StateNode
    {
        internal Type StateType { get; }

        internal string GeneratedTypeName { get; private set; }

        internal NextStateKind? NextStateKind { get; private set; }

        internal Func<IStateContext, Task<bool>> NextStateCondition { get; private set; }

        internal StateNode NextStateNodeIfTrue { get; private set; }

        internal StateNode NextStateNodeElse { get; private set; }

        internal StateNode(Type stateType)
        {
            StateType = stateType;
        }

        public StateNode SetNext<T>(NextStateKind nextStateKind) where T : IState
        {
            NextStateKind = nextStateKind;
            return NextStateNodeIfTrue = new StateNode(typeof(T));
        }

        public StateNode SetNext(NextStateKind nextStateKind, Type nextState)
        {
            if (!typeof(IState).IsAssignableFrom(nextState))
            {
                throw new ArgumentException(nameof(nextState), $"Type must implement interface {nameof(IState)}");
            }

            NextStateKind = nextStateKind;
            return NextStateNodeIfTrue = new StateNode(nextState);
        }

        public StateNode SetNext(NextStateKind nextStateKind, StateNode nextStateNode)
        {
            NextStateKind = nextStateKind;
            return NextStateNodeIfTrue = nextStateNode;
        }

        public (StateNode IfTrueState, StateNode ElseState) SetNext(NextStateKind nextStateKind, Func<IStateContext, Task<bool>> condition, Type ifTrue, Type @else)
        {
            if (!typeof(IState).IsAssignableFrom(ifTrue))
            {
                throw new ArgumentException(nameof(ifTrue), $"Type must implement interface {nameof(IState)}");
            }

            if (!typeof(IState).IsAssignableFrom(@else))
            {
                throw new ArgumentException(nameof(@else), $"Type must implement interface {nameof(IState)}");
            }

            NextStateKind = nextStateKind;
            NextStateCondition = condition;
            NextStateNodeIfTrue = new StateNode(ifTrue);
            NextStateNodeElse = new StateNode(@else);

            return (NextStateNodeIfTrue, NextStateNodeElse);
        }

        public (StateNode IfTrueState, StateNode ElseState) SetNext(NextStateKind nextStateKind, Func<IStateContext, Task<bool>> condition, Type ifTrue, StateNode elseStateNode)
        {
            if (!typeof(IState).IsAssignableFrom(ifTrue))
            {
                throw new ArgumentException(nameof(ifTrue), $"Type must implement interface {nameof(IState)}");
            }

            NextStateKind = nextStateKind;
            NextStateCondition = condition;
            NextStateNodeIfTrue = new StateNode(ifTrue);
            NextStateNodeElse = elseStateNode;

            return (NextStateNodeIfTrue, NextStateNodeElse);
        }

        public (StateNode IfTrueState, StateNode ElseState) SetNext(NextStateKind nextStateKind, Func<IStateContext, Task<bool>> condition, StateNode ifTrueStateNode, Type @else)
        {
            if (!typeof(IState).IsAssignableFrom(@else))
            {
                throw new ArgumentException(nameof(@else), $"Type must implement interface {nameof(IState)}");
            }

            NextStateKind = nextStateKind;
            NextStateCondition = condition;
            NextStateNodeIfTrue = ifTrueStateNode;
            NextStateNodeElse = new StateNode(@else);

            return (NextStateNodeIfTrue, NextStateNodeElse);
        }

        public (StateNode IfTrueState, StateNode ElseState) SetNext(NextStateKind nextStateKind, Func<IStateContext, Task<bool>> condition, StateNode ifTrueStateNode, StateNode elseStateNode)
        {
            NextStateKind = nextStateKind;
            NextStateCondition = condition;
            NextStateNodeIfTrue = ifTrueStateNode;
            NextStateNodeElse = elseStateNode;

            return (NextStateNodeIfTrue, NextStateNodeElse);
        }

        public (StateNode IfTrueState, StateNode ElseState) SetNext<TIfTrue, TElse>(NextStateKind nextStateKind, Func<IStateContext, Task<bool>> condition)
            where TIfTrue : IState
            where TElse : IState
        {
            NextStateKind = nextStateKind;
            NextStateCondition = condition;
            NextStateNodeIfTrue = new StateNode(typeof(TIfTrue));
            NextStateNodeElse = new StateNode(typeof(TElse));

            return (NextStateNodeIfTrue, NextStateNodeElse);
        }

        internal void Register(ContainerBuilder containerBuilder)
        {
            if (GeneratedTypeName != null)
            {
                return;
            }

            GeneratedTypeName = $"{nameof(GeneratedState)}<{StateType.Name}" +
                                $"{(NextStateKind == null ? "" : $"->{NextStateKind}:{NextStateNodeIfTrue.StateType.Name}{(NextStateNodeElse == null ? "" : $"||{NextStateNodeElse.StateType.Name}")}")}>";

            containerBuilder.RegisterType<GeneratedState>()
                .WithParameter(
                    new ResolvedParameter(
                        (pi, ctx) => pi.ParameterType.IsAssignableFrom(typeof(IState)),
                        (pi, ctx) => ctx.ResolveNamed<IState>(StateType.Name)))
                .WithParameter(new TypedParameter(typeof(StateNode), this))
                .Named<IState>(GeneratedTypeName)
                .InstancePerUpdate();

            NextStateNodeIfTrue?.Register(containerBuilder);
            NextStateNodeElse?.Register(containerBuilder);
        }
    }
}
