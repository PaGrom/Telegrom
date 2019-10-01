namespace ShowMustNotGoOn.Core.Model.Callback.Navigate
{
    public sealed class NextNavigateCallbackQueryData : NavigateCallbackQueryData
    {
        public NextNavigateCallbackQueryData()
        {
            CallbackQueryType = CallbackQueryType.NavigateNext;
        }
    }
}
