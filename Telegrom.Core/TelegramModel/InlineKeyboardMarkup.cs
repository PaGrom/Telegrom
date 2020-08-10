using System.Collections.Generic;

namespace Telegrom.Core.TelegramModel
{
    /// <summary>
    /// This object represents an inline keyboard that appears right next to the <see cref="T:ShowMustNotGoOn.Core.TelegramModel.Message" /> it belongs to.
    /// </summary>
    public class InlineKeyboardMarkup : IReplyMarkup
    {
        /// <summary>
        /// Array of <see cref="T:ShowMustNotGoOn.Core.TelegramModel.InlineKeyboardButton" /> rows, each represented by an Array of <see cref="T:ShowMustNotGoOn.Core.TelegramModel.InlineKeyboardButton" />.
        /// </summary>
        public List<List<InlineKeyboardButton>> InlineKeyboard { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Telegrom.Core.TelegramModel.InlineKeyboardMarkup" /> class
        /// </summary>
        public InlineKeyboardMarkup()
            : this(new List<List<InlineKeyboardButton>>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Telegrom.Core.TelegramModel.InlineKeyboardMarkup" /> class with only one keyboard button
        /// </summary>
        /// <param name="inlineKeyboardButton">Keyboard button</param>
        public InlineKeyboardMarkup(InlineKeyboardButton inlineKeyboardButton)
            : this(new List<InlineKeyboardButton>
            {
                inlineKeyboardButton
            })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Telegrom.Core.TelegramModel.InlineKeyboardMarkup" /> class with a one-row keyboard
        /// </summary>
        /// <param name="inlineKeyboardRow">The inline keyboard row</param>
        public InlineKeyboardMarkup(
            List<InlineKeyboardButton> inlineKeyboardRow)
        {
            InlineKeyboard =
                new List<List<InlineKeyboardButton>>
                {
                    inlineKeyboardRow
                };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Telegrom.Core.TelegramModel.InlineKeyboardMarkup" /> class.
        /// </summary>
        /// <param name="inlineKeyboard">The inline keyboard.</param>
        public InlineKeyboardMarkup(
            List<List<InlineKeyboardButton>> inlineKeyboard)
        {
            InlineKeyboard = inlineKeyboard;
        }

        /// <summary>Generate an empty inline keyboard markup</summary>
        /// <returns>Empty inline keyboard markup</returns>
        public static InlineKeyboardMarkup Empty()
        {
            return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>());
        }

        /// <summary>
        /// Add new buttons row to the end of keyboard
        /// </summary>
        /// <param name="buttonsRow"></param>
        public void AddRow(List<InlineKeyboardButton> buttonsRow)
        {
            InlineKeyboard.Add(buttonsRow);
        }
    }
}
