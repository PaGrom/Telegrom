using System.Linq;
using AutoMapper;
using ShowMustNotGoOn.Core.Model;
using ShowMustNotGoOn.Core.Model.Callback;
using ShowMustNotGoOn.Core.Model.Callback.Navigate;
using Telegram.Bot.Types.Enums;

namespace ShowMustNotGoOn.TelegramService
{
    public class TelegramServiceMappingProfile : Profile
    {
        public TelegramServiceMappingProfile()
        {
            CreateMap<Telegram.Bot.Types.User, ShowMustNotGoOn.Core.Model.User>()
                .ForMember(dest => dest.TelegramId,
                    opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Id,
                    opt => opt.Ignore());

            CreateMap<Telegram.Bot.Types.Message, ShowMustNotGoOn.Core.Model.Message>()
                .ForMember(dest => dest.FromUser,
                    opt => opt.MapFrom(src => src.From))
                .ForMember(dest => dest.BotCommand,
                    opt =>
                    {
                        opt.PreCondition(src => src.Entities?.FirstOrDefault()?.Type == MessageEntityType.BotCommand);
                        opt.MapFrom(src => MapBotCommand(src.EntityValues.FirstOrDefault()));
                    })
                .ForMember(dest => dest.NextButton,
                    opt =>
                    {
                        opt.PreCondition(src => src.ReplyMarkup?.InlineKeyboard?
                                                    .SelectMany(k => k)
                                                    .SingleOrDefault(b => CallbackQueryDataSerializer.Deserialize(b.CallbackData)
                                                        is NextNavigateCallbackQueryData) != null);
                        opt.MapFrom(src => new ButtonCallbackQueryData
                        {
                            MessageId = src.MessageId,
                            CallbackQueryType = CallbackQueryType.NavigateNext,
                            Data = src.ReplyMarkup.InlineKeyboard
                                       .SelectMany(k => k)
                                       .Single(b => CallbackQueryDataSerializer.Deserialize(b.CallbackData)
                                           is NextNavigateCallbackQueryData)
                                       .CallbackData
                        });
                    });

            CreateMap<Telegram.Bot.Types.CallbackQuery, CallbackQuery>()
                .ForMember(dest => dest.FromUser,
                    opt => opt.MapFrom(src => src.From))
                .ForMember(dest => dest.Message,
                    opt => opt.MapFrom(src => src.Message))
                .ForMember(dest => dest.CallbackQueryDataId,
                    opt => opt.MapFrom(src => int.Parse(src.Data)));
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
