using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Segrom.JobObserver.TelegramBot.Application.Exceptions;
using Segrom.JobObserver.TelegramBot.KafkaConsumer.Abstractions;

namespace Segrom.JobObserver.TelegramBot.KafkaConsumer.Factories;

internal sealed class ConsumerFactory<TKey, TValue>(
	ILogger<ConsumerFactory<TKey, TValue>> logger,
	IConfiguration configuration,
	IDeserializer<TValue> deserializer
	): IConsumerFactory<TKey, TValue>
{
	private const short DEFAULT_REPLICATION_FACTOR = 1;
	private const int DEFAULT_PARTITIONS = 5;
	
	private readonly string _bootstrapServers = configuration.GetSection("KAFKA_BROKERS").Value
	                                            ?? throw new KafkaConsumerException("KAFKA_BROKERS not configured");
	
	public IConsumer<TKey, TValue> Create(string consumerGroupId, Action<ConsumerConfig>? configModifier = null)
	{
		var config = new ConsumerConfig
		{
			BootstrapServers = _bootstrapServers,
			GroupId = consumerGroupId,
			EnableAutoCommit = false,
			//AutoOffsetReset = AutoOffsetReset.Earliest,
		};
		configModifier?.Invoke(config);

		var consumer = new ConsumerBuilder<TKey, TValue>(config)
			.SetErrorHandler(ErrorHandler)
			.SetLogHandler(LogHandler)
			.SetValueDeserializer(deserializer)
			.Build();

		return consumer;
	}
		
	public async Task EnsureTopic(string topic)
	{
		try
		{
			var adminConfig = new AdminClientConfig { BootstrapServers = _bootstrapServers };
			var adminClient = new AdminClientBuilder(adminConfig).Build();
			
			await adminClient.CreateTopicsAsync(
				[
					new TopicSpecification
					{
						Name = topic,
						NumPartitions = DEFAULT_PARTITIONS,
						ReplicationFactor = DEFAULT_REPLICATION_FACTOR
					}
				], 
				new CreateTopicsOptions
				{
					RequestTimeout = TimeSpan.FromSeconds(10)
				});
            
			logger.LogInformation("[EnsureTopic] Topic created  {Topic} ({Partitions}/{Replication})", 
				topic, DEFAULT_PARTITIONS, DEFAULT_REPLICATION_FACTOR);
		}
		catch (CreateTopicsException ex)
		{
			if (ex.Results.Any(r => r.Error.Code == ErrorCode.TopicAlreadyExists))
			{
				logger.LogInformation("[EnsureTopic] Topic {Topic} already exists", topic);
			}
			else
			{
				logger.LogError(ex, "Failed to create topic {Topic}", topic);
				throw;
			}
		}
	}

	private void ErrorHandler(IConsumer<TKey, TValue> _, Error error) =>
		logger.LogError("[KafkaConsumerError]: {Error}", error.Reason);

	private void LogHandler(IConsumer<TKey, TValue> _, LogMessage message) =>
		logger.LogInformation("[KafkaConsumer]: {Message}", message);
}