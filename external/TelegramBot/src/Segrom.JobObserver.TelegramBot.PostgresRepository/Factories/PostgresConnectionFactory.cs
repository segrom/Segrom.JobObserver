using Microsoft.Extensions.Configuration;
using Npgsql;
using Segrom.JobObserver.TelegramBot.Application.Exceptions;
using Segrom.JobObserver.TelegramBot.PostgresRepository.Abstractions;

namespace Segrom.JobObserver.TelegramBot.PostgresRepository.Factories;

internal sealed class PostgresConnectionFactory(IConfiguration configuration) : IPostgresConnectionFactory
{
    private readonly string _connectionString = configuration.GetConnectionString("BotDb") 
                                                ?? throw new MessagesRepositoryException(
                                                    "PostgresVacancy connection string is null");

    public NpgsqlConnection CreateConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }
}