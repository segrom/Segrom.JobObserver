using Segrom.JobObserver.OzonService.Domain;

namespace Segrom.JobObserver.OzonService.Application.Abstractions;

public interface IOutboxRepository
{
	Task<List<OutboxRecord>> GetRecords(CancellationToken cancellationToken);
	Task DeleteRecords(List<OutboxRecord> records, CancellationToken cancellationToken);
}