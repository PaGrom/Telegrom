using System.Collections.Generic;
using AutoMapper;
using ShowMustNotGoOn.Core.TelegramModel;
using Telegram.Bot.Types.InputFiles;

namespace ShowMustNotGoOn.TelegramService
{
    public class TelegramMappingProfile : Profile
    {
        public TelegramMappingProfile()
        {
            CreateMap<Telegram.Bot.Types.User, User>();

            CreateMap<Telegram.Bot.Types.Update, Message>()
                .ForMember(dest => dest.MessageId,
                    opt => opt.MapFrom(src => src.CallbackQuery.Message.MessageId))
                .ForMember(dest => dest.Text,
                    opt => opt.MapFrom(src => src.Message.Text));

            CreateMap<Telegram.Bot.Types.Update, CallbackQuery>()
                .ForMember(dest => dest.Id,
                    opt => opt.MapFrom(src => src.CallbackQuery.Id))
                .ForMember(dest => dest.MessageId,
                    opt => opt.MapFrom(src => src.CallbackQuery.Message.MessageId))
                .ForMember(dest => dest.Data,
                    opt => opt.MapFrom(src => src.CallbackQuery.Data));

            CreateMap<SendMessageRequest, Telegram.Bot.Requests.SendMessageRequest>()
                .ConstructUsing(r => new Telegram.Bot.Requests.SendMessageRequest(new Telegram.Bot.Types.ChatId(r.ChatId), r.Text));

            CreateMap<InlineKeyboardButton, Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton>()
                .ForMember(dest => dest.Text,
                    opt => opt.MapFrom(src => src.Text))
                .ForMember(dest => dest.CallbackData,
                    opt => opt.MapFrom(src => src.CallbackData));

            CreateMap<InlineKeyboardMarkup, Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup>()
                .ConstructUsing((m, ctx) =>
                    new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup(
                        ctx.Mapper.Map<IEnumerable<IEnumerable<Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton>>>(
                            m.InlineKeyboard)));

            CreateMap<SendPhotoRequest, Telegram.Bot.Requests.SendPhotoRequest>()
                .ConstructUsing((r, ctx) =>
                    new Telegram.Bot.Requests.SendPhotoRequest(new Telegram.Bot.Types.ChatId(r.ChatId), new InputOnlineFile(r.Photo))
                    {
                        Caption = r.Caption,
                        ReplyMarkup = ctx.Mapper.Map<Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup>(r.ReplyMarkup)
                    });
        }
    }
}
