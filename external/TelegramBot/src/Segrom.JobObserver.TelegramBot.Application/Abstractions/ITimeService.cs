namespace Segrom.JobObserver.TelegramBot.Application.Abstractions;

public interface ITimeService
{
	DateTime GetCurrentUtcTime();
}