using System.Threading;
using System.Threading.Tasks;
using ShowMustNotGoOn.Core.Model;
using Telegrom.Core.TelegramModel;
using Telegrom.StateMachine;
using Telegrom.StateMachine.Attributes;

namespace ShowMustNotGoOn.Core.States
{
    public sealed class GenerateSendPhotoWithFirstTvShowRequest : StateBase
    {
        private readonly IStateContext _stateContext;
        private readonly ITvShowsService _tvShowsService;

        [Input]
        public TvShowInfo CurrentTvShowInfo { get; set; }

        [Output]
        public SendPhotoRequest SendPhotoRequest { get; set; }

        public GenerateSendPhotoWithFirstTvShowRequest(IStateContext stateContext, ITvShowsService tvShowsService)
        {
            _stateContext = stateContext;
            _tvShowsService = tvShowsService;
        }

        public override async Task OnEnter(CancellationToken cancellationToken)
        {
            var tvShowDescription = await _tvShowsService.GetTvShowDescriptionAsync(CurrentTvShowInfo.MyShowsId, cancellationToken);

            SendPhotoRequest = new SendPhotoRequest(_stateContext.UpdateContext.SessionContext.User.Id, tvShowDescription.Image)
            {
                Caption = $"{tvShowDescription.Title} / {tvShowDescription.TitleOriginal}"
            };
        }
    }
}
