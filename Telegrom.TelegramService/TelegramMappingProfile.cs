using System.Collections.Generic;
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

            CreateMap<KeyboardButton, Telegram.Bot.Types.ReplyMarkups.KeyboardButton>()
                .ForMember(dest => dest.Text,
                    opt => opt.MapFrom(src => src.Text));

            CreateMap<ReplyKeyboardMarkup, Telegram.Bot.Types.ReplyMarkups.ReplyKeyboardMarkup>()
                .ConstructUsing((m, ctx) =>
                    new Telegram.Bot.Types.ReplyMarkups.ReplyKeyboardMarkup(
                        ctx.Mapper.Map<IEnumerable<IEnumerable<Telegram.Bot.Types.ReplyMarkups.KeyboardButton>>>(
                            m.Keyboard)));

            CreateMap<ReplyKeyboardRemove, Telegram.Bot.Types.ReplyMarkups.ReplyKeyboardRemove>();

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

            CreateMap<SendMessageRequest, Telegram.Bot.Requests.SendMessageRequest>()
                .ConstructUsing((r, ctx) =>
                    new Telegram.Bot.Requests.SendMessageRequest(new Telegram.Bot.Types.ChatId(r.ChatId), r.Text)
                    {
                        ReplyMarkup = r.KeyboardMarkup switch
                        {
                            ReplyKeyboardMarkup markup => ctx.Mapper.Map<Telegram.Bot.Types.ReplyMarkups.ReplyKeyboardMarkup>(markup),
                            ReplyKeyboardRemove remove => ctx.Mapper.Map<Telegram.Bot.Types.ReplyMarkups.ReplyKeyboardRemove>(remove),
                            _ => null
                        }
                    });

            CreateMap<SendPhotoRequest, Telegram.Bot.Requests.SendPhotoRequest>()
                .ForMember(dest => dest.ReplyMarkup, 
                    opt => opt.Ignore())
                .ConstructUsing((r, ctx) =>
                    new Telegram.Bot.Requests.SendPhotoRequest(new Telegram.Bot.Types.ChatId(r.ChatId), new Telegram.Bot.Types.InputFiles.InputOnlineFile(r.Photo))
                    {
                        Caption = r.Caption,
                        ReplyMarkup = ctx.Mapper.Map<Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup>(r.ReplyMarkup)
                    });

            CreateMap<AnswerCallbackQueryRequest, Telegram.Bot.Requests.AnswerCallbackQueryRequest>()
                .ConstructUsing(r => new Telegram.Bot.Requests.AnswerCallbackQueryRequest(r.CallbackQueryId));

            CreateMap<EditMessageMediaRequest, Telegram.Bot.Requests.EditMessageMediaRequest>()
                .ConstructUsing(r =>
                    new Telegram.Bot.Requests.EditMessageMediaRequest(new Telegram.Bot.Types.ChatId(r.ChatId),
                        r.MessageId,
                        new Telegram.Bot.Types.InputMediaPhoto(new Telegram.Bot.Types.InputMedia(r.Photo))));

            CreateMap<EditMessageCaptionRequest, Telegram.Bot.Requests.EditMessageCaptionRequest>()
                .ConstructUsing((r, ctx) =>
                    new Telegram.Bot.Requests.EditMessageCaptionRequest(new Telegram.Bot.Types.ChatId(r.ChatId),
                        r.MessageId, r.Caption)
                    {
                        ReplyMarkup = ctx.Mapper.Map<Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup>(r.ReplyMarkup)
                    });

            CreateMap<EditMessageReplyMarkupRequest, Telegram.Bot.Requests.EditMessageReplyMarkupRequest>()
                .ConstructUsing((r, ctx) =>
                    new Telegram.Bot.Requests.EditMessageReplyMarkupRequest(new Telegram.Bot.Types.ChatId(r.ChatId),
                        r.MessageId,
                        ctx.Mapper.Map<Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup>(r.ReplyMarkup)));

            CreateMap<DeleteMessageRequest, Telegram.Bot.Requests.DeleteMessageRequest>()
                .ConstructUsing(r =>
                    new Telegram.Bot.Requests.DeleteMessageRequest(new Telegram.Bot.Types.ChatId(r.ChatId),
                        r.MessageId));
        }
    }
}
