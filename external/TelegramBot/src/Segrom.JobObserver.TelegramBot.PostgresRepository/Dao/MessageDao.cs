namespace Segrom.JobObserver.TelegramBot.PostgresRepository.Dao;

internal sealed class MessageDao
{
	public int Id { get; set; }
	public long ChatId { get; set; }
	public long VacancyId { get; set; }
	public DateTimeOffset CreatedAt { get; set; }
}