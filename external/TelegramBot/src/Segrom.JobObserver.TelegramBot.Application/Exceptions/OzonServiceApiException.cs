namespace Segrom.JobObserver.TelegramBot.Application.Exceptions;

public sealed class OzonServiceApiException: Exception
{
	public OzonServiceApiException()
	{
	}

	public OzonServiceApiException(string? message) : base(message)
	{
	}

	public OzonServiceApiException(string? message, Exception? innerException) : base(message, innerException)
	{
	}
}