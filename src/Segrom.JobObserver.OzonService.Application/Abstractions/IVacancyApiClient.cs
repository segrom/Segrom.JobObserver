using Segrom.JobObserver.OzonService.Domain;

namespace Segrom.JobObserver.OzonService.Application.Abstractions;

public interface IVacancyApiClient
{
    Task<IReadOnlyList<Vacancy>> GetVacanciesAsync(int limit, int page, CancellationToken cancellationToken);
    Task GetFillVacancyInfoAsync(Vacancy vacancy, CancellationToken cancellationToken);
}