using Npgsql;

namespace Segrom.JobObserver.TelegramBot.PostgresRepository.Abstractions;

internal interface IPostgresConnectionFactory
{
    public NpgsqlConnection CreateConnection();
}