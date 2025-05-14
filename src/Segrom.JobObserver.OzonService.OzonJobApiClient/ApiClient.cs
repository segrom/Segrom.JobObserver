using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Segrom.JobObserver.OzonService.Application.Abstractions;
using Segrom.JobObserver.OzonService.Application.Exceptions;
using Segrom.JobObserver.OzonService.Domain;
using Segrom.JobObserver.OzonService.OzonJobApiClient.Dto;

namespace Segrom.JobObserver.OzonService.OzonJobApiClient;

internal sealed class ApiClient(
    HttpClient httpClient,
    ILogger<ApiClient> logger
    ): IVacancyApiClient
{
  // more info by https://job-api.ozon.ru/vacancy/119896782   
    private const string OzonApiBase = "https://job-api.ozon.ru";
    private const string OzonVacancyEndpoint = "vacancy";
    
    public async Task<IReadOnlyList<Vacancy>> GetVacanciesAsync(int limit, int page, CancellationToken cancellationToken)
    {
        try
        {
            var query = new Dictionary<string, string>
            {
                // { "query", "" },
                // { "department", "" },
                { "tech", "C#" },
                { "limit", limit.ToString() },
                { "page", page.ToString() }
            };
            var uri = new Uri(QueryHelpers.AddQueryString($"{OzonApiBase}/{OzonVacancyEndpoint}", query));
            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            using var response = await httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<VacanciesDto>(json);
            
            if (result is null)
            {
                logger.LogWarning("Result vacancies is null, content: {Raw}", json);
                return [];
            }
            
            logger.LogDebug("Receive {Count} vacancies, total count: {Total}", 
                result.Items.Count, result.Meta.TotalItems);
            
            return result.Items.Select( x => new Vacancy
            {
                Id = x.HhId,
                Title = x.Title,
                City = x.City,
                Department = x.Department,
                Employment = x.Employment,
                Experience = x.Experience
            }).ToList();
        }
        catch (Exception e)
        {
            throw new VacancyApiClientException("Failed get vacancies from job api", e);
        }
    }
    
    public async Task GetFillVacancyInfoAsync(Vacancy vacancy, CancellationToken cancellationToken)
    {
        try
        {
            if (vacancy.Info is not null) 
                return;
            
            var uri = new Uri($"{OzonApiBase}/{OzonVacancyEndpoint}/{vacancy.Id}");
            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            using var response = await httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<VacancyInfoDto>(json);
            
            if (result is null)
            {
                logger.LogWarning("Result vacancies is null, content: {Raw}", json);
                return;
            }
            
            vacancy.Info = new VacancyInfo(
                FullDescription: result.Description,
                Skills: result.Skills.Select(x=>x.Name).ToList(),
                Slug: result.Slug,
                Url: $"https://job.ozon.ru/vacancy/{result.Slug}");
        }
        catch (Exception e)
        {
            throw new VacancyApiClientException("Failed get vacancies from job api", e);
        }
    }
}