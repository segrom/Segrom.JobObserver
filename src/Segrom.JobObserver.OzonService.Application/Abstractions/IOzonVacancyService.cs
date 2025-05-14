using Segrom.JobObserver.OzonService.Domain;

namespace Segrom.JobObserver.OzonService.Application.Abstractions;

public interface IOzonVacancyService
{
    Task<IReadOnlyList<Vacancy>> GetVacanciesAsync(int limit, int page, CancellationToken cancellationToken);
    Task UpdateVacancies(CancellationToken cancellationToken);
    Task FillVacancyInfo(long id, CancellationToken contextCancellationToken);
}