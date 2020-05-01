using System;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ShowMustNotGoOn.Core.Extensions
{
	public static class UpdateExtensions
	{
		public static User GetUser(this Update update)
		{
			return update.Type switch
			{
				UpdateType.Message => update.Message.From,
				UpdateType.InlineQuery => update.InlineQuery.From,
				UpdateType.ChosenInlineResult => update.ChosenInlineResult.From,
				UpdateType.CallbackQuery => update.CallbackQuery.From,
				UpdateType.EditedMessage => update.EditedMessage.From,
				UpdateType.ChannelPost => update.ChannelPost.From,
				UpdateType.EditedChannelPost => update.EditedChannelPost.From,
				UpdateType.ShippingQuery => update.ShippingQuery.From,
				UpdateType.PreCheckoutQuery => update.PreCheckoutQuery.From,
				_ => throw new InvalidOperationException()
			};
		}
	}
}
