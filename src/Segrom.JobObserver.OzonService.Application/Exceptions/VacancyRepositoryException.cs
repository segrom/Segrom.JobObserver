using System.Runtime.Serialization;

namespace Segrom.JobObserver.OzonService.Application.Exceptions;

public sealed class VacancyRepositoryException: Exception
{
    public VacancyRepositoryException()
    {
    }

    public VacancyRepositoryException(string? message) : base(message)
    {
    }

    public VacancyRepositoryException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}