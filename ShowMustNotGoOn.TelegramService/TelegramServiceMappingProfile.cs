using AutoMapper;

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
                    opt => opt.MapFrom(src => src.From));
        }
    }
}
