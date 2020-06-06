using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Telegrom.StateMachine.Attributes;

namespace Telegrom.StateMachine.Builder
{
    internal class GeneratedState : IState
    {
        private readonly IState _current;
        private readonly StateNode _stateNode;
        private readonly IStateContext _stateContext;

        public GeneratedState(
            IState current,
            StateNode stateNode,
            IStateContext stateContext)
        {
            _current = current;
            _stateNode = stateNode;
            _stateContext = stateContext;
        }

        public async Task OnEnter(CancellationToken cancellationToken)
        {
            RestoreInputAttributes();

            await _current.OnEnter(cancellationToken);

            SaveOutputStateAttributes();

            await MoveNextAsync(NextStateKind.AfterOnEnter);
        }

        public async Task Handle(CancellationToken cancellationToken)
        {
            await _current.Handle(cancellationToken);

            SaveOutputStateAttributes();

            await MoveNextAsync(NextStateKind.AfterHandle);
        }

        public async Task OnExit(CancellationToken cancellationToken)
        {
            await _current.OnExit(cancellationToken);

            SaveOutputStateAttributes();

            await MoveNextAsync(NextStateKind.AfterOnExit);
        }

        private async Task MoveNextAsync(NextStateKind nextStateKind)
        {
            if (_stateNode.NextStateKind != nextStateKind)
            {
                return;
            }

            _stateContext.StateMachineContext.MoveTo(_stateNode.ElseState.StateNode.GeneratedTypeName);

            foreach (var ifState in _stateNode.IfStates)
            {
                if (await ifState.Condition(_stateContext))
                {
                    _stateContext.StateMachineContext.MoveTo(ifState.StateNode.GeneratedTypeName);
                    break;
                }
            }
        }

        private void RestoreInputAttributes()
        {
            var type = _current.GetType();
            var props = type.GetProperties().Where(
                prop => Attribute.IsDefined(prop, typeof(InputAttribute)));

            foreach (var prop in props)
            {
                var propName = prop.Name;
                var propType = prop.PropertyType;

                if (!_stateContext.Attributes.ContainsKey(propName)
                    || _stateContext.Attributes[propName].type != propType)
                {
                    prop.SetValue(_current, null);
                    continue;
                }

                prop.SetValue(_current, _stateContext.Attributes[propName].value);
            }
        }

        private void SaveOutputStateAttributes()
        {
            var type = _current.GetType();
            var props = type.GetProperties().Where(
                prop => Attribute.IsDefined(prop, typeof(OutputAttribute)));

            foreach (var prop in props)
            {
                var propName = prop.Name;
                var propType = prop.PropertyType;
                var propValue = prop.GetValue(_current);

                if (propValue == null)
                {
                    continue;
                }

                _stateContext.Attributes[propName] = (propType, propValue);
            }
        }
    }
}
