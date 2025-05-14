namespace Segrom.JobObserver.OzonService.Application.Exceptions;

public sealed class OutboxRepositoryException: Exception
{
	public OutboxRepositoryException()
	{
	}

	public OutboxRepositoryException(string? message) : base(message)
	{
	}

	public OutboxRepositoryException(string? message, Exception? innerException) : base(message, innerException)
	{
	}
}