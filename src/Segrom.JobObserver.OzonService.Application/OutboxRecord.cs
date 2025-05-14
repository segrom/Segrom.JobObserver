namespace Segrom.JobObserver.OzonService.Application;

public record OutboxRecord(int Id, string Topic, byte[] Value, byte[]? Key = null);