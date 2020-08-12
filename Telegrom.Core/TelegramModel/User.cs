namespace Telegrom.Core.TelegramModel
{
    public sealed class User
    {
        /// <summary>
        /// Telegram User Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Telegram User Name
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Telegram User First Name
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Telegram User Last Name
        /// </summary>
        public string LastName { get; set; }
    }
}
