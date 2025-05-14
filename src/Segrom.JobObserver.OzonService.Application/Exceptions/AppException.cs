using System.Runtime.Serialization;

namespace Segrom.JobObserver.OzonService.Application.Exceptions;

public sealed class AppException: Exception
{
    public AppException()
    {
    }

    public AppException(string? message) : base(message)
    {
    }

    public AppException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}