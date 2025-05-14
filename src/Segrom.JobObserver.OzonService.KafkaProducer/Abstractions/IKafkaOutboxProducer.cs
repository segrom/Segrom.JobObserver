using Segrom.JobObserver.OzonService.Application;

namespace Segrom.JobObserver.OzonService.KafkaProducer.Abstractions;

public interface IKafkaOutboxProducer: IDisposable
{
	public Task ProduceNewRecords(List<OutboxRecord> orders, CancellationToken cancellationToke);
}