namespace ShowMustNotGoOn.Core.TelegramModel
{
    /// <summary>
    /// Edit captions and game messages sent by the bot. On success the edited <see cref="T:ShowMustNotGoOn.Core.TelegramModel.Message" /> is returned.
    /// </summary>
    public sealed class EditMessageCaptionRequest : Request
    {
        /// <summary>
        /// Identifier of the sent message
        /// </summary>
        public int MessageId { get; }

        /// <summary>
        /// New caption of the message
        /// </summary>
        public string Caption { get; set; }

        /// <summary>
        /// Keyboard
        /// </summary>
        public InlineKeyboardMarkup ReplyMarkup { get; set; }

        /// <summary>
        /// Initializes a new request with chatId, messageId and new caption
        /// </summary>
        /// <param name="chatId">Unique identifier for the target chat or username of the target channel</param>
        /// <param name="messageId">Identifier of the sent message</param>
        /// <param name="caption">New caption of the message</param>
        public EditMessageCaptionRequest(int chatId, int messageId, string caption = null)
        {
            this.ChatId = chatId;
            this.MessageId = messageId;
            this.Caption = caption;
        }
    }
}
