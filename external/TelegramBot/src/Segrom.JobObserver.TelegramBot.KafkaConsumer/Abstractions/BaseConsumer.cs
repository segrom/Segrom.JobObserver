using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Segrom.JobObserver.TelegramBot.KafkaConsumer.Options;

namespace Segrom.JobObserver.TelegramBot.KafkaConsumer.Abstractions;

internal abstract class BaseConsumer<TKey, TValue>(
	IConsumerFactory<TKey, TValue> consumerFactory,
	IOptions<KafkaConsumerOptions> options,
	ILogger logger
): BackgroundService
{
	protected abstract string Topic { get; }
	
	protected sealed override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		await Task.Delay(1, stoppingToken).ConfigureAwait(false);

		await consumerFactory.EnsureTopic(Topic);
		
		while (!stoppingToken.IsCancellationRequested)
		{
			using var consumer = consumerFactory.Create(options.Value.ConsumerGroupId, config =>
			{
				config.EnableAutoCommit = false;
				/*config.AutoOffsetReset = AutoOffsetReset.Latest;*/
			});
			
			logger.LogInformation("Consumer {ConsumerName} start running", GetType().Name);
			
			try
			{
				consumer.Subscribe(Topic);
				await StartConsuming(consumer, stoppingToken);
			}
			catch (Exception ex)
			{
				await Task.Delay(1000, stoppingToken);
				logger.LogError(ex, "[{ConsumerType}] Consume error", GetType().Name);
			}
			finally
			{
				consumer.Unsubscribe();
			}
		}
	}
	
	protected abstract Task HandleMessage(ConsumeResult<TKey,TValue> result, CancellationToken cancellationToken);

	private async Task StartConsuming(IConsumer<TKey, TValue> consumer,
		CancellationToken cancellationToken)
	{
		while (!cancellationToken.IsCancellationRequested)
		{
			var result = consumer.Consume(cancellationToken);
			var consumeTime = DateTime.UtcNow;
			if (result is null) 
				continue;

			await HandleMessage(result, cancellationToken);
			consumer.Commit(result);
			
			var remainingTime = options.Value.ConsumerDelay - (int)DateTime.UtcNow.Subtract(consumeTime).TotalMilliseconds;
			if (remainingTime <= 0)
				continue;
			await Task.Delay(remainingTime, cancellationToken);
		}
	}
}