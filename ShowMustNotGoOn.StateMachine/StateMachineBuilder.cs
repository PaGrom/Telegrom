using Autofac;
using ShowMustNotGoOn.StateMachine.Builder;

namespace ShowMustNotGoOn.StateMachine
{
    public class StateMachineBuilder
    {
        private readonly ContainerBuilder _builder;
        private StateNode _initStateNode;
        private StateNode _defaultStateNode;

        public string InitStateName => _initStateNode?.GeneratedTypeName;
        public string DefaultStateName => _defaultStateNode?.GeneratedTypeName;

        public StateMachineBuilder(ContainerBuilder builder)
        {
            _builder = builder;
        }

        public IStateNode AddInit<TInit>() where TInit: IState
        {
            _initStateNode = new StateNode(typeof(TInit));
            _defaultStateNode = _initStateNode;

            return _initStateNode;
        }

        public void SetDefaultStateNode(IStateNode stateNode)
        {
            _defaultStateNode = (StateNode)stateNode;
        }

        public void Build()
        {
            _initStateNode.Register(_builder);
        }
    }
}
