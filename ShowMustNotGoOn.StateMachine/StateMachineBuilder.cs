using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;

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

        public IStateNode AddInit<TInit>() where TInit: IState
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

        IStateNode AddNextAfterOnEnter<TOnEnter>() where TOnEnter : IState;
        IStateNode AddNextAfterHandle<THandle>() where THandle : IState;
        IStateNode AddNextAfterOnExit<TOnExit>() where TOnExit : IState;

        IStateNode AddNextAfterOnEnter(IStateNode stateNode);
        IStateNode AddNextAfterHandle(IStateNode stateNode);
        IStateNode AddNextAfterOnExit(IStateNode stateNode);
    }

    public sealed class StateNode: IStateNode
    {
        public Type StateType { get; }
        public string GeneratedTypeName { get; private set; }

        public StateNode NextAfterOnEnter { get; private set; }
        public StateNode NextAfterHandle { get; private set; }
        public StateNode NextAfterOnExit { get; private set; }

        public StateNode(Type stateType)
        {
            StateType = stateType;
        }

        public IStateNode AddNextAfterOnEnter<TOnEnter>() where TOnEnter : IState
            => NextAfterOnEnter = new StateNode(typeof(TOnEnter));

        public IStateNode AddNextAfterHandle<THandle>() where THandle : IState
            => NextAfterOnEnter == null
                ? NextAfterHandle = new StateNode(typeof(THandle))
                : null;

        public IStateNode AddNextAfterOnExit<TOnExit>() where TOnExit : IState
            => NextAfterOnEnter == null && NextAfterHandle == null
                ? NextAfterOnExit = new StateNode(typeof(TOnExit))
                : null;

        public IStateNode AddNextAfterOnEnter(IStateNode stateNode)
            => NextAfterOnEnter = (StateNode)stateNode;

        public IStateNode AddNextAfterHandle(IStateNode stateNode)
            => NextAfterOnEnter == null
                ? NextAfterHandle = (StateNode)stateNode
                : null;

        public IStateNode AddNextAfterOnExit(IStateNode stateNode)
            => NextAfterOnEnter == null && NextAfterHandle == null
                ? NextAfterOnExit = (StateNode)stateNode
                : null;

        public void RegisterChildren(ContainerBuilder containerBuilder)
        {
            if (GeneratedTypeName != null)
            {
                return;
            }

            if (NextAfterOnEnter != null)
            {
                RegisterType(containerBuilder, typeof(GeneratedStateOnEnter), NextAfterOnEnter.StateType.Name);
                NextAfterOnEnter.RegisterChildren(containerBuilder);
                return;
            }

            if (NextAfterHandle != null)
            {
                RegisterType(containerBuilder, typeof(GeneratedStateHandle), NextAfterHandle.StateType.Name);
                NextAfterHandle.RegisterChildren(containerBuilder);
                return;
            }

            if (NextAfterOnExit != null)
            {
                RegisterType(containerBuilder, typeof(GeneratedStateOnExit), NextAfterOnExit.StateType.Name);
                NextAfterOnExit.RegisterChildren(containerBuilder);
                return;
            }

            RegisterType(containerBuilder, typeof(GeneratedStateOnEnter), "null");
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
                .Named<IState>(GeneratedTypeName);
        }

        //public IStateNode AddNextAfterOnEnter<TOnEnterIfTrue, TOnEnterIfFalse>(Func<CancellationToken, Task<bool>> condition)
        //    where TOnEnterIfTrue : IState
        //    where TOnEnterIfFalse: IState
        //    => NextAfterOnEnter = new StateNode(typeof(TOnEnter));
    }

    public sealed class GeneratedStateOnEnter: IState
    {
        private readonly IState _current;
        private readonly StateNode _stateNode;
        private readonly IStateContext _stateContext;

        public GeneratedStateOnEnter(IState current, StateNode stateNode, IStateContext stateContext)
        {
            _current = current;
            _stateNode = stateNode;
            _stateContext = stateContext;
        }

        public async Task OnEnter(CancellationToken cancellationToken)
        {
            await _current.OnEnter(cancellationToken);

            if (_stateNode.NextAfterOnEnter != null)
            {
                var nextStateName = _stateNode.NextAfterOnEnter.GeneratedTypeName;
                _stateContext.StateMachineContext.MoveTo(nextStateName);
            }
        }
    }

    public sealed class GeneratedStateHandle : IState
    {
        private readonly IState _current;
        private readonly StateNode _stateNode;
        private readonly IStateContext _stateContext;

        public GeneratedStateHandle(IState current, StateNode stateNode, IStateContext stateContext)
        {
            _current = current;
            _stateNode = stateNode;
            _stateContext = stateContext;
        }

        public async Task<bool> Handle(CancellationToken cancellationToken)
        {
            var handled = await _current.Handle(cancellationToken);
            if (!handled)
            {
                return false;
            }

            if (_stateNode.NextAfterHandle != null)
            {
                var nextStateName = _stateNode.NextAfterHandle.GeneratedTypeName;
                _stateContext.StateMachineContext.MoveTo(nextStateName);
            }

            return true;
        }
    }

    public sealed class GeneratedStateOnExit : IState
    {
        private readonly IState _current;
        private readonly StateNode _stateNode;
        private readonly IStateContext _stateContext;

        public GeneratedStateOnExit(IState current, StateNode stateNode, IStateContext stateContext)
        {
            _current = current;
            _stateNode = stateNode;
            _stateContext = stateContext;
        }

        public async Task OnExit(CancellationToken cancellationToken)
        {
            await _current.OnExit(cancellationToken);

            if (_stateNode.NextAfterOnExit != null)
            {
                var nextStateName = _stateNode.NextAfterOnExit.GeneratedTypeName;
                _stateContext.StateMachineContext.MoveTo(nextStateName);
            }
        }
    }
}
