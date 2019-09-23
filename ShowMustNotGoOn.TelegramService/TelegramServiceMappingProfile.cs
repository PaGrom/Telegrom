using System.Linq;
using AutoMapper;
using Telegram.Bot.Types.Enums;

namespace ShowMustNotGoOn.TelegramService
{
    public class TelegramServiceMappingProfile : Profile
    {
        public TelegramServiceMappingProfile()
        {
            CreateMap<Telegram.Bot.Types.User, ShowMustNotGoOn.Core.Model.User>()
                .ForMember(dest => dest.TelegramId,
                    opt => opt.MapFrom(src => src.Id));

            CreateMap<Telegram.Bot.Types.Message, ShowMustNotGoOn.Core.Model.Message>()
                .ForMember(dest => dest.FromUser,
                    opt => opt.MapFrom(src => src.From))
                .ForMember(dest => dest.BotCommand,
                    opt =>
                    {
                        opt.PreCondition(src => src.Entities?.FirstOrDefault()?.Type == MessageEntityType.BotCommand);
                        opt.MapFrom(src => MapBotCommand(src.EntityValues.FirstOrDefault()));
                    });
        }

        public static ShowMustNotGoOn.Core.Model.BotCommandType? MapBotCommand(string botCommand)
        {
            switch (botCommand)
            {
                case "/start":
                    return ShowMustNotGoOn.Core.Model.BotCommandType.Start;
                default:
                    return null;
            }
        }
    }
}
