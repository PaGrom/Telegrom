using Telegrom.StateMachine;

namespace ShowMustNotGoOn.Core.States
{
    public class SendCantFindTvShowsMessage : SendMessage
    {
        public SendCantFindTvShowsMessage(IStateContext stateContext) : base(stateContext, "  сожалению, сериалов по вашему запросу не найдено") { }
    }
}
