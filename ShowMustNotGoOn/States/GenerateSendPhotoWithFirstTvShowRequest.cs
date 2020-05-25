using System.Threading;
using System.Threading.Tasks;
using ShowMustNotGoOn.Core.TelegramModel;
using ShowMustNotGoOn.DatabaseContext.Model;
using ShowMustNotGoOn.StateMachine;
using ShowMustNotGoOn.StateMachine.Attributes;

namespace ShowMustNotGoOn.States
{
    internal sealed class GenerateSendPhotoWithFirstTvShowRequest : StateBase
    {
        private readonly IStateContext _stateContext;

        [Input]
        public TvShow CurrentTvShow { get; set; }

        [Output]
        public SendPhotoRequest SendPhotoRequest { get; set; }

        public GenerateSendPhotoWithFirstTvShowRequest(IStateContext stateContext)
        {
            _stateContext = stateContext;
        }

        public override Task OnEnter(CancellationToken cancellationToken)
        {
            const string notFoundImage = "https://images-na.ssl-images-amazon.com/images/I/312yeogBelL._SX466_.jpg";

            if (string.IsNullOrEmpty(CurrentTvShow.Image))
            {
                CurrentTvShow.Image = notFoundImage;
            }

            SendPhotoRequest = new SendPhotoRequest(_stateContext.UpdateContext.SessionContext.User.Id, CurrentTvShow.Image)
            {
                Caption = $"{CurrentTvShow.Title} / {CurrentTvShow.TitleOriginal}"
            };

            return Task.CompletedTask;
        }
    }
}
