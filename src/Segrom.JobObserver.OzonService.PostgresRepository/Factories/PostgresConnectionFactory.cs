using Microsoft.Extensions.Configuration;
using Npgsql;
using Segrom.JobObserver.OzonService.Application.Exceptions;
using Segrom.JobObserver.OzonService.PostgresRepository.Abstractions;

namespace Segrom.JobObserver.OzonService.PostgresRepository.Factories;

internal sealed class PostgresConnectionFactory(IConfiguration configuration) : IPostgresConnectionFactory
{
    private readonly string _connectionString = configuration.GetConnectionString("PostgresVacancy") 
                                                ?? throw new VacancyRepositoryException(
                                                    "PostgresVacancy connection string is null");

    public NpgsqlConnection CreateConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }
}