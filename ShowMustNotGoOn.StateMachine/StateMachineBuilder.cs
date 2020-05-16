using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using ShowMustNotGoOn.Core.Extensions;

namespace ShowMustNotGoOn.StateMachine
{
    public class StateMachineBuilder
    {
        private readonly ContainerBuilder _builder;
        private StateNode _initStateNode;

        public string InitStateName => _initStateNode?.GeneratedTypeName;

        public StateMachineBuilder(ContainerBuilder builder)
        {
            _builder = builder;
        }

        public IStateNode AddInit<TInit>() where TInit: StateBase
        {
            _initStateNode = new StateNode(typeof(TInit));

            return _initStateNode;
        }

        public void Build()
        {
            _initStateNode.RegisterChildren(_builder);
        }
    }

    public interface IStateNode
    {
        string GeneratedTypeName { get; }

        IStateNode AddNextAfterOnEnter<TOnEnter>() where TOnEnter : StateBase;
        IStateNode AddNextAfterHandle<THandle>() where THandle : StateBase;
        IStateNode AddNextAfterOnExit<TOnExit>() where TOnExit : StateBase;

        IStateNode AddNextAfterOnEnter(IStateNode stateNode);
        IStateNode AddNextAfterHandle(IStateNode stateNode);
        IStateNode AddNextAfterOnExit(IStateNode stateNode);
    }

    public enum NextStateType
    {
        AfterOnEnter,
        AfterHandle,
        AfterOnExit
    }

    public sealed class StateNode: IStateNode
    {
        public Type StateType { get; }
        public string GeneratedTypeName { get; private set; }

        public StateNode NextStateNode { get; private set; }

        public NextStateType? NextStateType { get; private set; }

        public StateNode(Type stateType)
        {
            StateType = stateType;
        }

        public IStateNode AddNextAfterOnEnter<TOnEnter>() where TOnEnter : StateBase
        {
            NextStateType = StateMachine.NextStateType.AfterOnEnter;
            return NextStateNode = new StateNode(typeof(TOnEnter));
        }

        public IStateNode AddNextAfterHandle<THandle>() where THandle : StateBase
        {
            NextStateType = StateMachine.NextStateType.AfterHandle;
            return NextStateNode = new StateNode(typeof(THandle));
        }

        public IStateNode AddNextAfterOnExit<TOnExit>() where TOnExit : StateBase
        {
            NextStateType = StateMachine.NextStateType.AfterOnExit;
            return NextStateNode = new StateNode(typeof(TOnExit));
        }

        public IStateNode AddNextAfterOnEnter(IStateNode stateNode)
        {
            NextStateType = StateMachine.NextStateType.AfterOnEnter;
            return NextStateNode = (StateNode)stateNode;
        }

        public IStateNode AddNextAfterHandle(IStateNode stateNode)
        {
            NextStateType = StateMachine.NextStateType.AfterHandle;
            return NextStateNode = (StateNode)stateNode;
        }

        public IStateNode AddNextAfterOnExit(IStateNode stateNode)
        {
            NextStateType = StateMachine.NextStateType.AfterOnExit;
            return NextStateNode = (StateNode)stateNode;
        }

        public void RegisterChildren(ContainerBuilder containerBuilder)
        {
            if (GeneratedTypeName != null)
            {
                return;
            }

            var typeToGenerate = NextStateType switch
            {
                StateMachine.NextStateType.AfterOnEnter => typeof(GeneratedStateOnEnter),
                StateMachine.NextStateType.AfterHandle => typeof(GeneratedStateHandle),
                StateMachine.NextStateType.AfterOnExit => typeof(GeneratedStateOnExit),
                null => typeof(GeneratedStateOnEnter),
                _ => throw new ArgumentOutOfRangeException()
            };

            var nextStateTypeName = NextStateNode?.StateType?.Name ?? "null";

            RegisterType(containerBuilder, typeToGenerate, nextStateTypeName);

            NextStateNode?.RegisterChildren(containerBuilder);
        }

        private void RegisterType(ContainerBuilder containerBuilder, Type generatedType, string nextTypeName)
        {
            GeneratedTypeName = $"{generatedType.Name}<{StateType.Name},{nextTypeName}>";
            containerBuilder.RegisterType(generatedType)
                .WithParameter(
                    new ResolvedParameter(
                        (pi, ctx) => pi.ParameterType.IsAssignableFrom(typeof(IState)),
                        (pi, ctx) => ctx.ResolveNamed<IState>(StateType.Name)))
                .WithParameter(new TypedParameter(typeof(StateNode), this))
                .Named<IState>(GeneratedTypeName)
                .InstancePerUpdate();
        }

        //public IStateNode AddNextAfterOnEnter<TOnEnterIfTrue, TOnEnterIfFalse>(Func<CancellationToken, Task<bool>> condition)
        //    where TOnEnterIfTrue : StateBase
        //    where TOnEnterIfFalse: StateBase
        //    => NextAfterOnEnter = new StateNode(typeof(TOnEnter));
    }

    public abstract class GeneratedState : StateBase
    {
        protected readonly IState Current;
        protected readonly StateNode StateNode;
        protected readonly IStateContext StateContext;

        protected GeneratedState(IState current, StateNode stateNode, IStateContext stateContext)
        {
            Current = current;
            StateNode = stateNode;
            StateContext = stateContext;
        }

        protected void MoveNext()
        {
            if (StateNode.NextStateNode != null)
            {
                var nextStateName = StateNode.NextStateNode.GeneratedTypeName;
                StateContext.StateMachineContext.MoveTo(nextStateName);
            }
            else
            {
                StateContext.StateMachineContext.Reset();
            }
        }
    }

    public sealed class GeneratedStateOnEnter: GeneratedState
    {
        public GeneratedStateOnEnter(IState current, StateNode stateNode, IStateContext stateContext)
            : base(current, stateNode, stateContext)
        {
        }

        public override async Task<bool> OnEnter(CancellationToken cancellationToken)
        {
            if (!await Current.OnEnter(cancellationToken))
            {
                return false;
            }

            MoveNext();
            return true;
        }
    }

    public sealed class GeneratedStateHandle : GeneratedState
    {
        public GeneratedStateHandle(IState current, StateNode stateNode, IStateContext stateContext)
            : base(current, stateNode, stateContext)
        {
        }

        public override async Task<bool> Handle(CancellationToken cancellationToken)
        {
            if (!await Current.Handle(cancellationToken))
            {
                return false;
            }

            MoveNext();
            return true;
        }
    }

    public sealed class GeneratedStateOnExit : GeneratedState
    {
        public GeneratedStateOnExit(IState current, StateNode stateNode, IStateContext stateContext)
            : base(current, stateNode, stateContext)
        {
        }

        public override async Task<bool> OnExit(CancellationToken cancellationToken)
        {
            if (!await Current.OnExit(cancellationToken))
            {
                return false;
            }

            MoveNext();
            return true;
        }
    }
}
