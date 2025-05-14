namespace Segrom.JobObserver.TelegramBot.Domain;

public sealed class VacancyMessage
{
	public int Id { get; set; }
	public long ChatId { get; set; }
	public long VacancyId { get; set; }
}