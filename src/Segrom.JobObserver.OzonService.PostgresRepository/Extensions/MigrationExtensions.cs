using FluentMigrator.Runner;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Segrom.JobObserver.OzonService.Application.Exceptions;

namespace Segrom.JobObserver.OzonService.PostgresRepository.Extensions;

public static class MigrationExtensions
{
    public static IApplicationBuilder RunPostgresRepositoryMigrations(this IApplicationBuilder app)
    {
        if (Environment.GetEnvironmentVariable("SKIP_MIGRATIONS") != null) return app;
        var configuration = app.ApplicationServices.GetService<IConfiguration>();
        var connectionString = configuration!.GetConnectionString("PostgresVacancy")
                               ?? throw new VacancyRepositoryException("No connection string configured.");
		
        var serviceProvider = CreateMigrationServices(connectionString);
        using var scope = serviceProvider.CreateScope();
        var runner = serviceProvider.GetRequiredService<IMigrationRunner>();

        if (configuration.GetSection("ROLLBACK_MIGRATIONS").Value?.ToLower() == "true")
        {
            runner.MigrateDown(0);
        }
        else
        {
            runner.MigrateUp();
        }
		
        return app;
    }

    private static IServiceProvider CreateMigrationServices(string connectionString)
        => new ServiceCollection()
            .AddFluentMigratorCore()
            .ConfigureRunner(builder => builder
                .AddPostgres()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(typeof(MigrationExtensions).Assembly).For.Migrations()
                .ConfigureGlobalProcessorOptions(op => op.ProviderSwitches = "Force Quote=false"))
            .AddLogging(log => log.AddFluentMigratorConsole())
            .BuildServiceProvider(false);
}