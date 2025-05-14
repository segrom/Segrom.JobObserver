using Segrom.JobObserver.TelegramBot.Application.Abstractions;

namespace Segrom.JobObserver.TelegramBot.Application;

internal sealed class TimeService: ITimeService
{
	public DateTime GetCurrentUtcTime() => DateTime.UtcNow;
}