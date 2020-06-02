using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShowMustNotGoOn.Core.TelegramModel;
using ShowMustNotGoOn.DatabaseContext.Extensions;
using ShowMustNotGoOn.DatabaseContext.Model;
using ShowMustNotGoOn.StateMachine;
using ShowMustNotGoOn.StateMachine.Attributes;

namespace ShowMustNotGoOn.States
{
    internal sealed class GenerateTvShowsBotMessage : StateBase
    {
        private readonly IStateContext _stateContext;
        private readonly DatabaseContext.DatabaseContext _databaseContext;

        [Input]
        public List<TvShow> TvShows { get; set; }

        [Output]
        public TvShow CurrentTvShow { get; set; }

        [Output]
        public BotMessage BotMessage { get; set; }

        public GenerateTvShowsBotMessage(IStateContext stateContext,
            DatabaseContext.DatabaseContext databaseContext)
        {
            _stateContext = stateContext;
            _databaseContext = databaseContext;
        }

        public override async Task OnEnter(CancellationToken cancellationToken)
        {
            var messageTextString = ((Message)_stateContext.UpdateContext.Update).Text.Trim();

            var messageText = await _databaseContext.MessageTexts
                .AddIfNotExistsAsync(new MessageText 
                {
                    Text = messageTextString
                }, s => s.Text == messageTextString, cancellationToken);

            await _databaseContext.SaveChangesAsync(cancellationToken);

            BotMessage = new BotMessage
            {
                UserId = _stateContext.UpdateContext.SessionContext.User.Id,
                BotCommandType = null,
                MessageTextId = messageText.Id,
                MyShowsId = TvShows.First().Id
            };

            await _databaseContext.BotMessages.AddAsync(BotMessage, cancellationToken);
            await _databaseContext.SaveChangesAsync(cancellationToken);

            CurrentTvShow = TvShows.First();
        }
    }
}
