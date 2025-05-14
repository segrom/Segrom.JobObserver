using Microsoft.Extensions.Logging;
using Prometheus;
using Segrom.JobObserver.OzonService.Application.Abstractions;
using Segrom.JobObserver.OzonService.Application.Exceptions;
using Segrom.JobObserver.OzonService.Domain;

namespace Segrom.JobObserver.OzonService.Application;

internal sealed class OzonVacancyService(
    IVacancyApiClient apiClient,
    IVacancyRepository repository,
    ILogger<OzonVacancyService> logger
    ): IOzonVacancyService
{
    private const int ALL_VACANCIES_DISCHARGE_BATCH_SIZE = 2056;
    
    private static readonly Counter VacanciesRefreshCalls = Metrics.CreateCounter(
        "vacancies_refresh_calls_total",
        "Total number of UpdateVacancies method calls",
        new CounterConfiguration
        {
            LabelNames = ["status", "category"]
        });
    
    private static readonly Counter CreatedVacanciesCounter = Metrics.CreateCounter(
        "vacancies_created_total",
        "Total number of created vacancies",
        new CounterConfiguration
        {
            LabelNames = ["category"]
        });
    
    private static readonly Counter UpdatedVacanciesCounter = Metrics.CreateCounter(
        "vacancies_updated_total",
        "Total number of updated vacancies",
        new CounterConfiguration
        {
            LabelNames = ["category"]
        });
    
    private static readonly Histogram VacanciesUpdateDuration = Metrics.CreateHistogram(
            "vacancies_update_call_duration_seconds", 
            "Time spent executing vacancies update method");
    
    private static readonly Histogram GetAllVacanciesDuration = Metrics.CreateHistogram(
        "vacancies_get_all_call_duration_seconds", 
        "Time spent executing get all vacancies method");
    
    public async Task<IReadOnlyList<Vacancy>> GetVacanciesAsync(int limit, int page, CancellationToken cancellationToken)
    {
        try
        {
            return await repository.GetVacancies(
                new VacanciesQuery(limit, page), cancellationToken);
        }
        catch (Exception e)
        {
            throw new AppException("Failed to get vacancies", e);
        }
    }
    
    public async Task UpdateVacancies(CancellationToken cancellationToken)
    {
        try
        {
            using var _ = VacanciesUpdateDuration.NewTimer();
            var newVacancies = await GetAllVacanciesFromApi(cancellationToken);
            var oldVacancies = await GetAllVacanciesFromRepository(cancellationToken);
            var toUpdate = new List<Vacancy>();
            var toCreate = newVacancies
                .Where(v => oldVacancies.All(x => x.Id != v.Id))
                .ToList();

            foreach (var oldVacancy in oldVacancies)
            {
                var newVacancy = newVacancies.FirstOrDefault(x => x.Id == oldVacancy.Id);
                if (newVacancy is null)
                {
                    oldVacancy.IsClosed = true;
                    toUpdate.Add(oldVacancy);
                    continue;
                }

                if (oldVacancy.GetHashCode() != newVacancy.GetHashCode())
                {
                    toUpdate.Add(newVacancy);
                }
            }
            
            if (toCreate.Count > 0)
            {
                await repository.CreateVacanciesAsync(toCreate, cancellationToken);
                CreatedVacanciesCounter.WithLabels("vacancies").Inc(toCreate.Count);
                logger.LogInformation("{CreatedCount} vacancies created", toCreate.Count);
            }
            if (toUpdate.Count > 0)
            {
                await repository.UpdateVacanciesAsync(toUpdate, cancellationToken);
                UpdatedVacanciesCounter.WithLabels("vacancies").Inc(toUpdate.Count);
                logger.LogInformation("{UpdatedCount} vacancies updated", toUpdate.Count);
            }
            
            VacanciesRefreshCalls.WithLabels("success", "vacancies").Inc();
        }
        catch (Exception e)
        {
            VacanciesRefreshCalls.WithLabels("failed", "vacancies").Inc();
            throw new AppException($"Failed to update vacancies ({e.Message})", e);
        }
    }

    public async Task FillVacancyInfo(long id, CancellationToken cancellationToken)
    {
        try
        {
            var vacancies = await repository.GetVacancies(
                new VacanciesQuery(5, 0, Ids: [id]),
                cancellationToken);
            var vacancy = vacancies.FirstOrDefault();
            if (vacancy is null)
            {
                throw new Exception($"Vacancy not found ({id})");
            }
            
            await apiClient.GetFillVacancyInfoAsync(vacancy, cancellationToken);
            await repository.UpdateVacanciesAsync([vacancy], cancellationToken);
            
            logger.LogInformation("Vacancy ({Id}) info filled!", id);
        }
        catch (Exception e)
        {
            throw new AppException($"Failed fill vacancy ({id}) info", e);
        }
    }

    private async Task<List<Vacancy>> GetAllVacanciesFromRepository(CancellationToken cancellationToken)
    {
        using var _ = GetAllVacanciesDuration.NewTimer();
        var page = 0;
        var result = new List<Vacancy>();
        IReadOnlyList<Vacancy> batch;
        do
        {
            batch = await repository.GetVacancies(
                new VacanciesQuery(ALL_VACANCIES_DISCHARGE_BATCH_SIZE, page),
                cancellationToken);
            result.AddRange(batch);
        } while (batch.Count == ALL_VACANCIES_DISCHARGE_BATCH_SIZE);
        return result;
    }
    
    private async Task<List<Vacancy>> GetAllVacanciesFromApi(CancellationToken cancellationToken)
    {
        var page = 0;
        var result = new List<Vacancy>();
        IReadOnlyList<Vacancy> batch;
        do
        {
            batch = await apiClient.GetVacanciesAsync(
                ALL_VACANCIES_DISCHARGE_BATCH_SIZE, page,
                cancellationToken);
            result.AddRange(batch);
        } while (batch.Count == ALL_VACANCIES_DISCHARGE_BATCH_SIZE);
        return result;
    }
}