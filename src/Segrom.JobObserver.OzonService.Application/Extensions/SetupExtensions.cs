using Microsoft.Extensions.DependencyInjection;
using Segrom.JobObserver.OzonService.Application.Abstractions;

namespace Segrom.JobObserver.OzonService.Application.Extensions;

public static class SetupExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddTransient<IOzonVacancyService, OzonVacancyService>();
        services.AddTransient<ITimeService, TimeService>();
        return services;
    }
}