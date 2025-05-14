using Npgsql;

namespace Segrom.JobObserver.OzonService.PostgresRepository.Abstractions;

internal interface IPostgresConnectionFactory
{
    public NpgsqlConnection CreateConnection();
}