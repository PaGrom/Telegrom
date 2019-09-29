namespace ShowMustNotGoOn.Core.Model.CallbackQuery
{
    public sealed class NextNavigateCallbackQueryData : NavigateCallbackQueryData
    {
        public NextNavigateCallbackQueryData()
        {
            CallbackQueryType = CallbackQueryType.Next;
        }
    }
}
