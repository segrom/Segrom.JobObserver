using Confluent.Kafka;

namespace Segrom.JobObserver.TelegramBot.KafkaConsumer.Abstractions;

internal interface IConsumerFactory<TKey, TValue>
{
	IConsumer<TKey, TValue> Create(string consumerGroupId, Action<ConsumerConfig>? configModificator = null);
	Task EnsureTopic(string topic);
}