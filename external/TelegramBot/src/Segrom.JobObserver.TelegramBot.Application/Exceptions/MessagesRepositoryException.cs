namespace Segrom.JobObserver.TelegramBot.Application.Exceptions;

public sealed class MessagesRepositoryException: Exception
{
	public MessagesRepositoryException()
	{
	}

	public MessagesRepositoryException(string? message) : base(message)
	{
	}

	public MessagesRepositoryException(string? message, Exception? innerException) : base(message, innerException)
	{
	}
}