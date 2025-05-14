using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Segrom.JobObserver.TelegramBot.Application.Abstractions;
using Segrom.JobObserver.TelegramBot.Domain;
using Segrom.JobObserver.TelegramBot.KafkaConsumer.Abstractions;
using Segrom.JobObserver.TelegramBot.KafkaConsumer.Options;

namespace Segrom.JobObserver.TelegramBot.KafkaConsumer;

internal sealed class VacancyNewConsumer(
	IBotService service,
	IConsumerFactory<string, List<Vacancy>> consumerFactory,
	IOptions<KafkaConsumerOptions> options,
	ILogger<VacancyNewConsumer> logger
	): BaseConsumer<string, List<Vacancy>>(consumerFactory, options, logger)
{
	protected override string Topic => options.Value.VacancyNewKafkaTopic;

	protected override async Task HandleMessage(
		ConsumeResult<string, List<Vacancy>> result,
		CancellationToken cancellationToken)
	{
		logger.LogInformation("Received NewVacancies: {Count}", result.Message.Value.Count);
		await service.HandleNewVacancies(result.Message.Value, cancellationToken);
	}
}