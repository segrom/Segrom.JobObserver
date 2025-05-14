using Segrom.JobObserver.OzonService.Domain;

namespace Segrom.JobObserver.OzonService.Application.Abstractions;

public interface IVacancyRepository
{
    Task CreateVacanciesAsync(IReadOnlyList<Vacancy> vacancies, CancellationToken cancellationToken);
    Task UpdateVacanciesAsync(IReadOnlyList<Vacancy> vacancies, CancellationToken cancellationToken);
    Task<IReadOnlyList<Vacancy>> GetVacancies(VacanciesQuery query, CancellationToken cancellationToken);
}