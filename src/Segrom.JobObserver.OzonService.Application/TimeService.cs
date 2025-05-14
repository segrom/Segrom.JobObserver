using Segrom.JobObserver.OzonService.Application.Abstractions;

namespace Segrom.JobObserver.OzonService.Application;

internal sealed class TimeService: ITimeService
{
	public DateTime GetCurrentUtcTime() => DateTime.UtcNow;
}