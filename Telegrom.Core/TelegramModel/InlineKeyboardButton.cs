namespace Telegrom.Core.TelegramModel
{
    /// <summary>
    /// This object represents one button of an inline keyboard.
    /// </summary>
    public class InlineKeyboardButton
    {
        /// <summary>
        /// Label text on the button
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Optional. Data to be sent in a callback query to the bot when button is pressed
        /// </summary>
        public string CallbackData { get; set; }

        /// <summary>
        /// Creates an inline keyboard button that sends <see cref="T:ShowMustNotGoOn.Core.TelegramModel.CallbackQuery" /> to bot when pressed
        /// </summary>
        /// <param name="text">Label text on the button</param>
        /// <param name="callbackData">Data to be sent in a <see cref="T:ShowMustNotGoOn.Core.TelegramModel.CallbackQuery" /> to the bot when button is pressed, 1-64 bytes</param>
        public static InlineKeyboardButton WithCallbackData(
            string text,
            string callbackData)
        {
            return new InlineKeyboardButton
            {
                Text = text,
                CallbackData = callbackData
            };
        }
    }
}
