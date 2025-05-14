using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Segrom.JobObserver.OzonService.Application;
using Segrom.JobObserver.OzonService.KafkaProducer.Abstractions;

namespace Segrom.JobObserver.OzonService.KafkaProducer;

internal sealed class KafkaOutboxProducer: IKafkaOutboxProducer
{
	private readonly ILogger<KafkaOutboxProducer> _logger;
	private readonly IProducer<byte[]?, byte[]> _producer;

	public KafkaOutboxProducer(
		IConfiguration configuration,
		ILogger<KafkaOutboxProducer> logger)
	{
		_logger = logger;
		var brokers = configuration.GetSection("KAFKA_BROKERS").Value 
		              ?? throw new Exception("KAFKA_BROKERS not configured");
		
		_producer = new ProducerBuilder<byte[]?, byte[]>(
				new ProducerConfig
				{
					BootstrapServers = brokers,
					Acks = Acks.All,
					Partitioner = Partitioner.ConsistentRandom
				})
			.SetLogHandler(LogHandler).SetErrorHandler(ErrorHandler)
			.Build();
	}
	
	public async Task ProduceNewRecords(List<OutboxRecord> records, CancellationToken cancellationToken)
	{
		var tasks = records.Select(x 
			=> _producer.ProduceAsync(x.Topic, new Message<byte[]?, byte[]>
			{
				Key = x.Key ?? null,
				Value = x.Value
			}, cancellationToken)
		).ToArray();
		await Task.WhenAll(tasks);
		_logger.LogInformation("Produce {Count} records", records.Count);
	}
	
	private void LogHandler(IProducer<byte[]?, byte[]> producer, LogMessage message)
	{
		_logger.LogInformation("[KafkaOutboxProducer] {Message}]", message);
	}
	
	private void ErrorHandler(IProducer<byte[]?, byte[]> producer, Error error)
	{
		_logger.LogError("[KafkaOutboxProducer] {Code}: {Reason}", error.Code, error.Reason);
	}

	public void Dispose()
	{
		_producer.Dispose();
	}
}