namespace Segrom.JobObserver.TelegramBot.Application.Exceptions;

public sealed class KafkaConsumerException: Exception
{
	public KafkaConsumerException()
	{
	}

	public KafkaConsumerException(string? message) : base(message)
	{
	}

	public KafkaConsumerException(string? message, Exception? innerException) : base(message, innerException)
	{
	}
}