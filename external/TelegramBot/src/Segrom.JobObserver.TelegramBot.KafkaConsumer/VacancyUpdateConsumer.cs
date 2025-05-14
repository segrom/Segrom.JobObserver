using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Segrom.JobObserver.TelegramBot.Application.Abstractions;
using Segrom.JobObserver.TelegramBot.Domain;
using Segrom.JobObserver.TelegramBot.KafkaConsumer.Abstractions;
using Segrom.JobObserver.TelegramBot.KafkaConsumer.Options;

namespace Segrom.JobObserver.TelegramBot.KafkaConsumer;

internal sealed class VacancyUpdateConsumer(
	IBotService service,
	IConsumerFactory<string, List<Vacancy>> consumerFactory,
	IOptions<KafkaConsumerOptions> options,
	ILogger<VacancyUpdateConsumer> logger
	): BaseConsumer<string, List<Vacancy>>(consumerFactory, options, logger)
{
	protected override string Topic => options.Value.VacancyUpdateKafkaTopic;

	protected override async Task HandleMessage(
		ConsumeResult<string, List<Vacancy>> result,
		CancellationToken cancellationToken)
	{
		logger.LogInformation("Received UpdateVacancies: {Count}", result.Message.Value.Count);
		await service.HandleUpdateVacancies(result.Message.Value, cancellationToken);
	}
}