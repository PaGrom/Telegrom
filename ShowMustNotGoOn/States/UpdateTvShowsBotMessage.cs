using System.Threading;
using System.Threading.Tasks;
using ShowMustNotGoOn.DatabaseContext.Model;
using ShowMustNotGoOn.StateMachine;
using ShowMustNotGoOn.StateMachine.Attributes;

namespace ShowMustNotGoOn.States
{
    internal sealed class UpdateTvShowsBotMessage : StateBase
    {
        private readonly DatabaseContext.DatabaseContext _databaseContext;

        [Input]
        public BotMessage BotMessage { get; set; }

        public UpdateTvShowsBotMessage(DatabaseContext.DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public override async Task OnEnter(CancellationToken cancellationToken)
        {
            //_databaseContext.BotMessages.Update(BotMessage);
            await _databaseContext.SaveChangesAsync(cancellationToken);
        }
    }
}
