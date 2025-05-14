using Microsoft.Extensions.DependencyInjection;
using Segrom.JobObserver.OzonService.Application.Abstractions;

namespace Segrom.JobObserver.OzonService.OzonJobApiClient.Extensions;

public static class SetupExtensions
{
    public static IServiceCollection AddOzonJobApiClient(this IServiceCollection services)
    {
        services.AddHttpClient();
        services.AddTransient<IVacancyApiClient, ApiClient>();
        return services;
    }
}