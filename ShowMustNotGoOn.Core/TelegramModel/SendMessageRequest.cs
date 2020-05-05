namespace ShowMustNotGoOn.Core.TelegramModel
{
    /// <summary>
    /// Send text messages
    /// </summary>
    public sealed class SendMessageRequest : Request
    {
        /// <summary>Initializes a new request with chatId and text</summary>
        /// <param name="chatId">Unique identifier for the target chat or username of the target channel</param>
        /// <param name="text">Text of the message to be sent</param>
        public SendMessageRequest(int chatId, string text)
        {
            ChatId = chatId;
            Text = text;
        }

        /// <summary>
        /// Text of the message to be sent
        /// </summary>
        public string Text { get; }
    }
}
