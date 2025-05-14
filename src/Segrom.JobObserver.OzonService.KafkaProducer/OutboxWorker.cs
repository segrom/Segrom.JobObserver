using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Segrom.JobObserver.OzonService.Application.Abstractions;
using Segrom.JobObserver.OzonService.Application.Options;
using Segrom.JobObserver.OzonService.KafkaProducer.Abstractions;

namespace Segrom.JobObserver.OzonService.KafkaProducer;

internal sealed class OutboxWorker(
	ILogger<OutboxWorker> logger,
	IOptions<OutboxOptions> options,
	IKafkaOutboxProducer producer,
	IServiceProvider serviceProvider
	): BackgroundService
{
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		await Task.Yield();
		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				await HandleRecords(stoppingToken);
			}
			catch (Exception e)
			{
				logger.LogError(e, "Error while handling records: {Message}, {Stacktrace}", e.Message, e.StackTrace);
				await Task.Delay(1000, stoppingToken);
			}
		}
	}
	
	private async Task HandleRecords( CancellationToken cancellationToken)
	{
		while (!cancellationToken.IsCancellationRequested)
		{
			await Task.Delay(options.Value.WorkerPeriod, cancellationToken);
			await using var scope = serviceProvider.CreateAsyncScope();
			var repository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
			
			var records = await repository.GetRecords(cancellationToken);
			if (records.Count <= 0) continue;
			await producer.ProduceNewRecords(records, cancellationToken);
			await repository.DeleteRecords(records, cancellationToken);
		}
	}
}