using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Segrom.JobObserver.OzonService.Application.Abstractions;
using Segrom.JobObserver.OzonService.PostgresRepository.Abstractions;
using Segrom.JobObserver.OzonService.PostgresRepository.Factories;

namespace Segrom.JobObserver.OzonService.PostgresRepository.Extensions;

public static class SetupExtensions
{
    public static IServiceCollection AddPostgresRepository(this IServiceCollection services)
    {
        services.AddSingleton<IPostgresConnectionFactory, PostgresConnectionFactory>();
        services.AddTransient<IVacancyRepository, VacancyRepository>();
        services.AddTransient<IOutboxFiller, OutboxRepository>();
        services.AddTransient<IOutboxRepository, OutboxRepository>();
        return services;
    }
}