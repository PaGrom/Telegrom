using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Telegrom.Core;

namespace Telegrom.StateMachine
{
    internal sealed class SessionStateAttributesService : ISessionStateAttributesService
    {
        private readonly ISessionAttributesService _sessionAttributesService;

        public SessionStateAttributesService(ISessionAttributesService sessionAttributesService)
        {
            _sessionAttributesService = sessionAttributesService;
        }

        public async Task CreateOrUpdateStateAttributeAsync(string name, Type type, object obj, CancellationToken cancellationToken)
        {
            var typeName = type.AssemblyQualifiedName;
            var stateAttribute =
                (await _sessionAttributesService.GetAllByTypeAsync<StateAttribute>(cancellationToken))
                .SingleOrDefault(a => a.Name.Equals(name) && a.TypeName.Equals(typeName))
                ?? new StateAttribute
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    TypeName = typeName
                };

            stateAttribute.Object = JsonConvert.SerializeObject(obj);

            await _sessionAttributesService.SaveOrUpdateSessionAttributeAsync(stateAttribute, cancellationToken);
        }

        public async IAsyncEnumerable<(string name, Type type, object obj)> GetAllStateAttributesAsync([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var attributes = await _sessionAttributesService.GetAllByTypeAsync<StateAttribute>(cancellationToken);

            foreach (var attribute in attributes)
            {
                var type = Type.GetType(attribute.TypeName);
                yield return (attribute.Name, type, JsonConvert.DeserializeObject(attribute.Object, type));
            }
        }

        public async Task RemoveAllStateAttributes(CancellationToken cancellationToken)
        {
            var attributes = await _sessionAttributesService.GetAllByTypeAsync<StateAttribute>(cancellationToken);

            foreach (var attribute in attributes)
            {
                await _sessionAttributesService.RemoveSessionAttributeAsync(attribute, cancellationToken);
            }
        }
    }
}
