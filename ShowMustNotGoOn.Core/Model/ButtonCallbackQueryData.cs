using ShowMustNotGoOn.Core.Model.Callback;

namespace ShowMustNotGoOn.Core.Model
{
    public sealed class ButtonCallbackQueryData
    {
        public int Id { get; set; }
        public User User { get; set; }
        public int MessageId { get; set; }
        public CallbackQueryType CallbackQueryType { get; set; }
        public string Data { get; set; }
    }
}
