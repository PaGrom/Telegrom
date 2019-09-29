namespace ShowMustNotGoOn.Core.Model.CallbackQuery
{
    public sealed class PrevNavigateCallbackQueryData : NavigateCallbackQueryData
    {
        public PrevNavigateCallbackQueryData()
        {
            CallbackQueryType = CallbackQueryType.Prev;
        }
    }
}
