namespace Segrom.JobObserver.OzonService.Application.Abstractions;

public interface ITimeService
{
	DateTime GetCurrentUtcTime();
}