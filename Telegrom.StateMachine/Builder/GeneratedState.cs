using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegrom.Core;
using Telegrom.StateMachine.Attributes;

namespace Telegrom.StateMachine.Builder
{
    internal class GeneratedState : IState
    {
        private readonly IState _current;
        private readonly StateNode _stateNode;
        private readonly IStateContext _stateContext;
        private readonly ISessionStateAttributesService _sessionStateAttributesService;

        public GeneratedState(
            IState current,
            StateNode stateNode,
            IStateContext stateContext,
            ISessionStateAttributesService sessionStateAttributesService)
        {
            _current = current;
            _stateNode = stateNode;
            _stateContext = stateContext;
            _sessionStateAttributesService = sessionStateAttributesService;
        }

        public async Task OnEnter(CancellationToken cancellationToken)
        {
            await RestoreInputAttributesAsync(cancellationToken);

            await _current.OnEnter(cancellationToken);

            await SaveOutputStateAttributesAsync(cancellationToken);

            await MoveNextAsync(NextStateKind.AfterOnEnter);
        }

        public async Task Handle(CancellationToken cancellationToken)
        {
            await _current.Handle(cancellationToken);

            await SaveOutputStateAttributesAsync(cancellationToken);

            await MoveNextAsync(NextStateKind.AfterHandle);
        }

        public async Task OnExit(CancellationToken cancellationToken)
        {
            await _current.OnExit(cancellationToken);

            await SaveOutputStateAttributesAsync(cancellationToken);

            await MoveNextAsync(NextStateKind.AfterOnExit);
        }

        private async Task MoveNextAsync(NextStateKind nextStateKind)
        {
            if (_stateNode.NextStateKind != nextStateKind)
            {
                return;
            }

            _stateContext.StateMachineContext.MoveTo(_stateNode.DefaultState.StateNode.StateName);

            foreach (var ifState in _stateNode.IfStates)
            {
                if (await ifState.Condition(_stateContext))
                {
                    _stateContext.StateMachineContext.MoveTo(ifState.StateNode.StateName);
                    break;
                }
            }
        }

        private async Task RestoreInputAttributesAsync(CancellationToken cancellationToken)
        {
            await foreach (var (attributeName, attributeType, obj) in _sessionStateAttributesService.GetAllStateAttributesAsync(cancellationToken))
            {
                _stateContext.Attributes[attributeName] = (attributeType, obj);
            }

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

        private async Task SaveOutputStateAttributesAsync(CancellationToken cancellationToken)
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

                await _sessionStateAttributesService.CreateOrUpdateStateAttributeAsync(propName, propType, propValue, cancellationToken);
            }
        }
    }
}
