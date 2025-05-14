namespace Segrom.JobObserver.TelegramBot.KafkaConsumer.Options;

public sealed class KafkaConsumerOptions
{
	public int ConsumerDelay { get; set; }
	public string ConsumerGroupId { get; set; } = "TelegramBot";
	public string VacancyNewKafkaTopic { get; set; } = "new_vacancies_events";
	public string VacancyUpdateKafkaTopic { get; set; } = "update_vacancies_events";
}