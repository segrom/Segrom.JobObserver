using Segrom.JobObserver.TelegramBot.Client.Abstractions;
using Segrom.JobObserver.TelegramBot.Client.Utils;
using Segrom.JobObserver.TelegramBot.Domain;
using Telegram.Bot.Extensions;
using Telegram.Bot.Types.ReplyMarkups;

namespace Segrom.JobObserver.TelegramBot.Client.Messages;

internal sealed class VacancyHtmlMessage(Vacancy vacancy): BaseSerializedMessage
{
	protected override string Serialize() =>
		$"""
		 {(vacancy.IsClosed ? "<s>" : "")}
		 <b>{vacancy.Title}</b>
		 
		 📌 [{vacancy.City}]
		 
		 ⚒️ {vacancy.Department}
		 
		 ⏲️ {vacancy.Employment}
		 
		 <i>{vacancy.Experience}</i>
		 {GetInfo()}
		 {(vacancy.IsClosed ? "</s>" : "")}
		 """;

	private string GetInfo() =>
		vacancy.Info is null
			? ""
			: $"""
			   <b>Полное описание:</b> 
			   {MarkupHelper.EscapeUnsupportedHtmlTags(vacancy.Info.FullDescription)}
			   
			   <b>Скилы:</b>
			   {string.Join(", ", vacancy.Info.Skills)}
			   
			   <i>{vacancy.Info.Url}</i>
			   """;
	
	public override InlineKeyboardButton? GetMarkup()
	{
		if (vacancy.Info is null)
		{
			return new InlineKeyboardButton(
				"Запросить больше информации",
				$"fill_info:{vacancy.Id}");
		}

		return null;
	}
}