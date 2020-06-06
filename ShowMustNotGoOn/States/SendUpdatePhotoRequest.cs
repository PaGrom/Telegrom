using System.Threading;
using System.Threading.Tasks;
using ShowMustNotGoOn.Core.Model;
using Telegrom.Core.TelegramModel;
using Telegrom.StateMachine;
using Telegrom.StateMachine.Attributes;

namespace ShowMustNotGoOn.States
{
    internal class SendUpdatePhotoRequest : StateBase
    {
        private readonly IStateContext _stateContext;

        [Input]
        public TvShow CurrentTvShow { get; set; }

        [Input]
        public InlineKeyboardMarkup InlineKeyboardMarkup { get; set; }

        public SendUpdatePhotoRequest(IStateContext stateContext)
        {
            _stateContext = stateContext;
        }

        public override async Task OnEnter(CancellationToken cancellationToken)
        {
            var userId = _stateContext.UpdateContext.SessionContext.User.Id;

            var callbackQuery = _stateContext.UpdateContext.Update as CallbackQuery;

            var answerCallbackQueryRequest = new AnswerCallbackQueryRequest(callbackQuery.Id);

            await _stateContext.UpdateContext.SessionContext.PostRequestAsync(answerCallbackQueryRequest, cancellationToken);

            const string notFoundImage = "https://images-na.ssl-images-amazon.com/images/I/312yeogBelL._SX466_.jpg";

            if (string.IsNullOrEmpty(CurrentTvShow.Image))
            {
                CurrentTvShow.Image = notFoundImage;
            }

            var editMessageMediaRequest = new EditMessageMediaRequest(userId, callbackQuery.MessageId, CurrentTvShow.Image);

            await _stateContext.UpdateContext.SessionContext.PostRequestAsync(editMessageMediaRequest, cancellationToken);

            var editCaptionRequest = new EditMessageCaptionRequest(userId, callbackQuery.MessageId, $"{ CurrentTvShow.Title } / { CurrentTvShow.TitleOriginal}")
            {
                ReplyMarkup = InlineKeyboardMarkup
            };

            await _stateContext.UpdateContext.SessionContext.PostRequestAsync(editCaptionRequest, cancellationToken);
        }
    }
}
