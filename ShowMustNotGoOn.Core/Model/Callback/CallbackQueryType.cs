using System;

namespace ShowMustNotGoOn.Core.Model.Callback
{
    [Serializable]
    public enum CallbackQueryType
    {
        NavigateNext,
        NavigatePrev,
        SubscribeEndOfShow,
        UnsubscribeEndOfShow
    }
}
