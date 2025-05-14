using System.Runtime.Serialization;

namespace Segrom.JobObserver.OzonService.Application.Exceptions;

public sealed class VacancyApiClientException: Exception
{
    public VacancyApiClientException()
    {
    }

    public VacancyApiClientException(string? message) : base(message)
    {
    }

    public VacancyApiClientException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}