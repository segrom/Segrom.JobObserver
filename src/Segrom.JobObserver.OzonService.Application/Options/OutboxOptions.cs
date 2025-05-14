namespace Segrom.JobObserver.OzonService.Application.Options;

public sealed class OutboxOptions
{
	public int WorkerPeriod { get; set; }
	public int TakenLifetime { get; set; }
	public int BachSize { get; set; }
}