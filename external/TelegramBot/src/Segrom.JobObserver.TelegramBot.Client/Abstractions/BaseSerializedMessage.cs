using Telegram.Bot.Types.ReplyMarkups;

namespace Segrom.JobObserver.TelegramBot.Client.Abstractions;

internal abstract class BaseSerializedMessage
{
	protected abstract string Serialize();
	public abstract InlineKeyboardButton? GetMarkup();
	public override string ToString() => Serialize();
}