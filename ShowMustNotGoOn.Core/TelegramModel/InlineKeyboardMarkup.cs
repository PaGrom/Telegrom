using System.Collections.Generic;

namespace ShowMustNotGoOn.Core.TelegramModel
{
    /// <summary>
    /// This object represents an inline keyboard that appears right next to the <see cref="T:ShowMustNotGoOn.Core.TelegramModel.Message" /> it belongs to.
    /// </summary>
    public class InlineKeyboardMarkup
    {
        /// <summary>
        /// Array of <see cref="T:ShowMustNotGoOn.Core.TelegramModel.InlineKeyboardButton" /> rows, each represented by an Array of <see cref="T:ShowMustNotGoOn.Core.TelegramModel.InlineKeyboardButton" />.
        /// </summary>
        public IEnumerable<IEnumerable<InlineKeyboardButton>> InlineKeyboard { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ShowMustNotGoOn.Core.TelegramModel.InlineKeyboardMarkup" /> class with only one keyboard button
        /// </summary>
        /// <param name="inlineKeyboardButton">Keyboard button</param>
        public InlineKeyboardMarkup(InlineKeyboardButton inlineKeyboardButton)
            : this(new InlineKeyboardButton[1]
            {
                inlineKeyboardButton
            })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ShowMustNotGoOn.Core.TelegramModel.InlineKeyboardMarkup" /> class with a one-row keyboard
        /// </summary>
        /// <param name="inlineKeyboardRow">The inline keyboard row</param>
        public InlineKeyboardMarkup(
            IEnumerable<InlineKeyboardButton> inlineKeyboardRow)
        {
            this.InlineKeyboard =
                new IEnumerable<InlineKeyboardButton>[1]
                {
                    inlineKeyboardRow
                };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ShowMustNotGoOn.Core.TelegramModel.InlineKeyboardMarkup" /> class.
        /// </summary>
        /// <param name="inlineKeyboard">The inline keyboard.</param>
        public InlineKeyboardMarkup(
            IEnumerable<IEnumerable<InlineKeyboardButton>> inlineKeyboard)
        {
            this.InlineKeyboard = inlineKeyboard;
        }

        /// <summary>Generate an empty inline keyboard markup</summary>
        /// <returns>Empty inline keyboard markup</returns>
        public static InlineKeyboardMarkup Empty()
        {
            return new InlineKeyboardMarkup(new InlineKeyboardButton[0][]);
        }
    }
}
