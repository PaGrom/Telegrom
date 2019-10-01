namespace ShowMustNotGoOn.Core.Model.Callback.Subscription
{
    public sealed class EndOfShowSubscriptionCallbackQueryData : SubscriptionCallbackQueryData
    {
        public EndOfShowSubscriptionCallbackQueryData()
        {
            CallbackQueryType = CallbackQueryType.SubscribeEndOfShow;
        }
    }
}
