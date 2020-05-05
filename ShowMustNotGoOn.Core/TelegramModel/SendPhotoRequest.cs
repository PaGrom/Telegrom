namespace ShowMustNotGoOn.Core.TelegramModel
{
    /// <summary>
    /// Send photos
    /// </summary>
    public sealed class SendPhotoRequest : Request
    {
        /// <summary>
        /// Photo to send
        /// </summary>
        public string Photo { get; }

        /// <summary>
        /// Photo caption (may also be used when resending photos by file_id), 0-1024 characters
        /// </summary>
        public string Caption { get; set; }

        /// <summary>
        /// Keyboard
        /// </summary>
        public InlineKeyboardMarkup ReplyMarkup { get; set; }

        /// <summary>Initializes a new request with chatId and photo</summary>
        /// <param name="chatId">Unique identifier for the target chat or username of the target channel</param>
        /// <param name="photo">Photo to send</param>
        public SendPhotoRequest(int chatId, string photo)
        {
            this.ChatId = chatId;
            this.Photo = photo;
        }
    }
}
