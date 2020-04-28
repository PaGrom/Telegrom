using System.Linq;
using AutoMapper;
using ShowMustNotGoOn.DatabaseContext.Model;
using Telegram.Bot.Types.Enums;

namespace ShowMustNotGoOn.TelegramService
{
    public class TelegramServiceMappingProfile : Profile
    {
        public TelegramServiceMappingProfile()
        {
            CreateMap<Telegram.Bot.Types.User, User>()
                .ForMember(dest => dest.TelegramId,
                    opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Id,
                    opt => opt.Ignore());

            CreateMap<Telegram.Bot.Types.Message, UserMessage>()
                .ForMember(dest => dest.User,
                    opt => opt.MapFrom(src => src.From))
                .ForMember(dest => dest.BotCommand,
                    opt =>
                    {
                        opt.PreCondition(src => src.Entities?.FirstOrDefault()?.Type == MessageEntityType.BotCommand);
                        opt.MapFrom(src => MapBotCommand(src.EntityValues.FirstOrDefault()));
                    });

            CreateMap<Telegram.Bot.Types.CallbackQuery, UserCallback>()
                .ForMember(dest => dest.User,
                    opt => opt.MapFrom(src => src.From))
                .ForMember(dest => dest.MessageId,
                    opt => opt.MapFrom(src => src.Message.MessageId))
                .ForMember(dest => dest.CallbackId,
                    opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.CallbackData,
                    opt => opt.MapFrom(src => src.Data));
        }

        public static BotCommandType? MapBotCommand(string botCommand)
        {
            return botCommand switch
            {
                "/start" => BotCommandType.Start,
                "/subscriptions" => BotCommandType.Subscriptions,
                _ => (BotCommandType?)null
            };
        }
    }
}
