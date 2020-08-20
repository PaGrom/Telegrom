using AutoMapper;
using Telegrom.Core.TelegramModel;

namespace Telegrom.TelegramService
{
    public class TelegramMappingProfile : Profile
    {
        public TelegramMappingProfile()
        {
            CreateMap<Telegram.Bot.Types.User, User>();

            CreateMap<Telegram.Bot.Types.Update, Message>()
                .ForMember(dest => dest.UpdateId,
                    opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.From,
                    opt => opt.MapFrom(src => src.GetUser()))
                .ForMember(dest => dest.MessageId,
                    opt => opt.MapFrom(src => src.Message.MessageId))
                .ForMember(dest => dest.Text,
                    opt => opt.MapFrom(src => src.Message.Text));

            CreateMap<Telegram.Bot.Types.Update, CallbackQuery>()
                .ForMember(dest => dest.UpdateId,
                    opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.From,
                    opt => opt.MapFrom(src => src.GetUser()))
                .ForMember(dest => dest.Id,
                    opt => opt.MapFrom(src => src.CallbackQuery.Id))
                .ForMember(dest => dest.MessageId,
                    opt => opt.MapFrom(src => src.CallbackQuery.Message.MessageId))
                .ForMember(dest => dest.Data,
                    opt => opt.MapFrom(src => src.CallbackQuery.Data));
        }
    }
}
