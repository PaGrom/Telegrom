namespace Telegrom.Core.TelegramModel
{
    /// <summary>
    /// Edit audio, document, photo, or video messages. On success the edited <see cref="T:ShowMustNotGoOn.Core.TelegramModel.Message" /> is returned.
    /// </summary>
    public sealed class EditMessageMediaRequest : Request
    {
        /// <summary>
        /// New photo content of the message
        /// </summary>
        public string Photo { get; }

        /// <summary>
        /// Identifier of the sent message
        /// </summary>
        public int MessageId { get; }

        /// <summary>
        /// Initializes a new request with chatId, messageId and new media
        /// </summary>
        /// <param name="chatId">Unique identifier for the target chat or username of the target channel</param>
        /// <param name="messageId">Identifier of the sent message</param>
        /// <param name="photo">New photo content of the message</param>
        public EditMessageMediaRequest(int chatId, int messageId, string photo)
        {
            this.ChatId = chatId;
            this.MessageId = messageId;
            this.Photo = photo;
        }
    }
}
