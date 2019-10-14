namespace ShowMustNotGoOn.Core.Model
{
    public sealed class UserCallback
    {
        public User User { get; set; }

        public int MessageId { get; set; }

        public string CallbackId { get; set; }

        public string CallbackData { get; set; }
    }
}
