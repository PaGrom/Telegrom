using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ShowMustNotGoOn.DatabaseContext.Model;
using ShowMustNotGoOn.StateMachine.Attributes;

namespace ShowMustNotGoOn.StateMachine.Builder
{
    internal class GeneratedState : IState
    {
        private readonly IState _current;
        private readonly StateNode _stateNode;
        private readonly IStateContext _stateContext;
        private readonly DatabaseContext.DatabaseContext _databaseContext;

        public GeneratedState(
            IState current,
            StateNode stateNode,
            IStateContext stateContext,
            DatabaseContext.DatabaseContext databaseContext)
        {
            _current = current;
            _stateNode = stateNode;
            _stateContext = stateContext;
            _databaseContext = databaseContext;
        }

        public async Task OnEnter(CancellationToken cancellationToken)
        {
            await FillInputAttributesAsync(cancellationToken);

            await _current.OnEnter(cancellationToken);

            await PersistOutputStateAttributesAsync(cancellationToken);

            await MoveNextAsync(NextStateType.AfterOnEnter);
        }

        public async Task Handle(CancellationToken cancellationToken)
        {
            await _current.Handle(cancellationToken);

            await PersistOutputStateAttributesAsync(cancellationToken);

            await MoveNextAsync(NextStateType.AfterHandle);
        }

        public async Task OnExit(CancellationToken cancellationToken)
        {
            await _current.OnExit(cancellationToken);

            await PersistOutputStateAttributesAsync(cancellationToken);

            await MoveNextAsync(NextStateType.AfterOnExit);
        }

        private async Task MoveNextAsync(NextStateType nextStateType)
        {
            var conditionalNextStateNodes = _stateNode.ConditionalNextStateNodes
                .Where(n => n.NextStateType == nextStateType)
                .ToList();

            if (!conditionalNextStateNodes.Any())
            {
                return;
            }

            var nextConditionalStateFound = false;

            foreach (var conditionalNextStateNode in conditionalNextStateNodes)
            {
                if (await conditionalNextStateNode.Condition(_stateContext))
                {
                    _stateContext.StateMachineContext.MoveTo(conditionalNextStateNode.NextStateNode.GeneratedTypeName);
                    nextConditionalStateFound = true;
                    break;
                }
            }

            if (!nextConditionalStateFound)
            {
                _stateContext.StateMachineContext.Reset();
            }
        }

        private async Task FillInputAttributesAsync(CancellationToken cancellationToken)
        {
            var userId = _stateContext.UpdateContext.SessionContext.User.Id;
            var type = _current.GetType();
            var props = type.GetProperties().Where(
                prop => Attribute.IsDefined(prop, typeof(InputAttribute)));

            foreach (var prop in props)
            {
                var propName = prop.Name;
                var propType = prop.PropertyType;
                var propTypeName = propType.Name;

                var attribute = await _databaseContext.StateAttributes
                    .SingleOrDefaultAsync(a => a.UserId == userId
                                               && a.Name == propName
                                               && a.TypeName == propTypeName,
                        cancellationToken);

                if (attribute == null)
                {
                    prop.SetValue(_current, null);
                    continue;
                }

                var propValue = JsonConvert.DeserializeObject(attribute.Object, prop.PropertyType);

                prop.SetValue(_current, propValue);

                _stateContext.Attributes[propName] = (propType, propValue);
            }
        }

        private async Task PersistOutputStateAttributesAsync(CancellationToken cancellationToken)
        {
            var userId = _stateContext.UpdateContext.SessionContext.User.Id;
            var type = _current.GetType();
            var props = type.GetProperties().Where(
                prop => Attribute.IsDefined(prop, typeof(OutputAttribute)));

            foreach (var prop in props)
            {
                var propName = prop.Name;
                var propType = prop.PropertyType;
                var propTypeName = propType.Name;
                var propValue = prop.GetValue(_current);

                if (propValue == null)
                {
                    continue;
                }

                var oldAttribute = await _databaseContext.StateAttributes
                    .SingleOrDefaultAsync(a => a.UserId == userId
                                               && a.Name == propName
                                               && a.TypeName == propTypeName,
                        cancellationToken);

                if (oldAttribute != null)
                {
                    oldAttribute.Object = JsonConvert.SerializeObject(propValue);
                }
                else
                {
                    await _databaseContext.StateAttributes.AddAsync(
                        new StateAttribute
                        {
                            UserId = userId,
                            Name = propName,
                            TypeName = propTypeName,
                            Object = JsonConvert.SerializeObject(propValue)
                        }, cancellationToken);
                }

                _stateContext.Attributes[propName] = (propType, propValue);
            }

            await _databaseContext.SaveChangesAsync(cancellationToken);
        }
    }
}
