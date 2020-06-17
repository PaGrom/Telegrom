using System.Threading;
using System.Threading.Tasks;
using ShowMustNotGoOn.Core.Model;
using Telegrom.Core;
using Telegrom.StateMachine;
using Telegrom.StateMachine.Attributes;

namespace ShowMustNotGoOn.Core.States
{
    public sealed class UpdateTvShowsBotMessage : StateBase
    {
        private readonly ISessionAttributesService _sessionAttributesService;

        [Input]
        public BotMessage BotMessage { get; set; }

        public UpdateTvShowsBotMessage(ISessionAttributesService sessionAttributesService)
        {
            _sessionAttributesService = sessionAttributesService;
        }

        public override Task OnEnter(CancellationToken cancellationToken)
        {
            return _sessionAttributesService.SaveOrUpdateSessionAttributeAsync(BotMessage, cancellationToken);
        }
    }
}
