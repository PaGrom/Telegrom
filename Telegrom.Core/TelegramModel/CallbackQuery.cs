namespace Telegrom.Core.TelegramModel
{
    /// <summary>
    /// Telegram Callback Query class
    /// </summary>
    public sealed class CallbackQuery : Update
    {
        /// <summary>
        /// Unique identifier for this query
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Unique message identifier
        /// </summary>
        public int MessageId { get; set; }

        /// <summary>
        /// Data associated with the callback button
        /// </summary>
        public string Data { get; set; }
    }
}
