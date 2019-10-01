namespace ShowMustNotGoOn.Core.Model.Callback.Navigate
{
    public sealed class PrevNavigateCallbackQueryData : NavigateCallbackQueryData
    {
        public PrevNavigateCallbackQueryData()
        {
            CallbackQueryType = CallbackQueryType.NavigatePrev;
        }
    }
}
