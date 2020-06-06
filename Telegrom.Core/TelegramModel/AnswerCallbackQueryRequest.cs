namespace Telegrom.Core.TelegramModel
{
    /// <summary>
    /// Send answers to callback queries sent from inline keyboards
    /// </summary>
    public sealed class AnswerCallbackQueryRequest : Request
    {
        /// <summary>
        /// Unique identifier for the query to be answered
        /// </summary>
        public string CallbackQueryId { get; }

        /// <summary>Initializes a new request with callbackQueryId</summary>
        /// <param name="callbackQueryId">Unique identifier for the query to be answered</param>
        public AnswerCallbackQueryRequest(string callbackQueryId)
        {
            this.CallbackQueryId = callbackQueryId;
        }
    }
}
