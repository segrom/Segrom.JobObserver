namespace Segrom.JobObserver.TelegramBot.Application.Exceptions;

public sealed class BotClientException: Exception
{
	public BotClientException()
	{
	}

	public BotClientException(string? message) : base(message)
	{
	}

	public BotClientException(string? message, Exception? innerException) : base(message, innerException)
	{
	}
}