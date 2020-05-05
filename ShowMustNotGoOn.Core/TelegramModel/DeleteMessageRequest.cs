namespace ShowMustNotGoOn.Core.TelegramModel
{
    /// <summary>
    /// Delete a message, including service messages
    /// </summary>
    public sealed class DeleteMessageRequest : Request
    {
        /// <summary>
        /// Identifier of the sent message
        /// </summary>
        public int MessageId { get; }

        /// <summary>Initializes a new request with chatId and messageId</summary>
        /// <param name="chatId">Unique identifier for the target chat or username of the target channel</param>
        /// <param name="messageId">Identifier of the sent message</param>
        public DeleteMessageRequest(int chatId, int messageId)
        {
            this.ChatId = chatId;
            this.MessageId = messageId;
        }
    }
}
