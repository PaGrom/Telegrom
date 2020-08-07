namespace Telegrom.Core.TelegramModel
{
    /// <summary>
    /// This object represents one button of the reply keyboard. For simple text buttons String can be used instead of this object to specify text of the button.
    /// </summary>
    public class KeyboardButton
    {
        public string Text { get; set; }

        /// <summary>
        /// Initializes a new <see cref="KeyboardButton"/>
        /// </summary>
        public KeyboardButton()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyboardButton"/> class.
        /// </summary>
        /// <param name="text">Label text on the button</param>
        public KeyboardButton(string text)
        {
            Text = text;
        }

        /// <summary>
        /// Generate a keyboard button from text
        /// </summary>
        /// <param name="text">Button's text</param>
        /// <returns>Keyboard button</returns>
        public static implicit operator KeyboardButton(string text)
            => new KeyboardButton(text);
    }
}
