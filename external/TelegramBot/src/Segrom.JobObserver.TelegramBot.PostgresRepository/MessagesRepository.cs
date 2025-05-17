using System.Text;
using System.Text.Json;
using Dapper;
using Microsoft.Extensions.Logging;
using Segrom.JobObserver.TelegramBot.Application.Abstractions;
using Segrom.JobObserver.TelegramBot.Application.Exceptions;
using Segrom.JobObserver.TelegramBot.Domain;
using Segrom.JobObserver.TelegramBot.PostgresRepository.Abstractions;
using Segrom.JobObserver.TelegramBot.PostgresRepository.Dao;
using Segrom.JobObserver.TelegramBot.PostgresRepository.Extensions;

namespace Segrom.JobObserver.TelegramBot.PostgresRepository;

internal sealed class MessagesRepository(
    IPostgresConnectionFactory connectionFactory,
    ITimeService timeService,
    ILogger<MessagesRepository> logger): IMessagesRepository
{
     private const string MESSAGES_BY_VACANCY_SQL = 
        """
        SELECT id AS Id, chat_id AS ChatId, vacancy_id AS VacancyId, created_at AS CreatedAt
        FROM messages
        WHERE vacancy_id = @vacancy_id
        ORDER BY created_at;
        """;
     public async Task<IReadOnlyDictionary<long, List<VacancyMessage>>> GetMessagesByVacancies(IList<Vacancy> vacancies, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await using var connection = connectionFactory.CreateConnection();
        var results = new Dictionary<long, List<VacancyMessage>>();
        try
        {
            foreach (var vacancy in vacancies)
            {
                var cmd = new CommandDefinition(
                    MESSAGES_BY_VACANCY_SQL, 
                    new { vacancy_id = vacancy.Id },
                    cancellationToken: cancellationToken);
                var messages = await connection.QueryAsync<MessageDao>(cmd);
                results[vacancy.Id] = messages.Select(x=> x.ToDomain()).ToList();
            }
            return results;
        }
        catch (Exception e)
        {
            throw new MessagesRepositoryException("Failed get vacancy messages", e);
        }
    }

     
    /* variant with DISTINCT ON  for some reason:
        SELECT DISTINCT ON (chat_id) chat_id
        FROM messages
        ORDER BY created_at;
    */
    private const string ALL_CHATS_SQL = 
        """
        SELECT chat_id AS ChatId
        FROM messages
        GROUP BY chat_id
        ORDER BY created_at;
        """;
    public async Task<IReadOnlyList<long>> GetAllChats(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await using var connection = connectionFactory.CreateConnection();
        try
        {
            var cmd = new CommandDefinition(
                ALL_CHATS_SQL,
                cancellationToken: cancellationToken);
            var chats = await connection.QueryAsync<long>(cmd);
            return chats.ToList();
        }
        catch (Exception e)
        {
            throw new MessagesRepositoryException("Failed get vacancy messages", e);
        }
    }
    
    private const string INSERT_MESSAGES_SQL = 
        """
        INSERT INTO messages  
        (id, chat_id, vacancy_id, created_at)
        SELECT * 
        FROM unnest(
          @ids::INT[], 
          @chat_ids::BIGINT[], 
          @vacancy_ids::BIGINT[],
          @timestamps::TIMESTAMPTZ[]);
        """;
    public async Task AddMessages(IList<VacancyMessage> messages, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await using var connection = connectionFactory.CreateConnection();
        var ids = new int[messages.Count];
        var chatIds = new long[messages.Count];
        var vacancyIds = new long[messages.Count];
        var timestamps = new DateTimeOffset[messages.Count];
        var currentUtc = timeService.GetCurrentUtcTime();

        for (var i = 0; i < messages.Count; i++)
        {
            ids[i] = messages[i].Id;
            chatIds[i] = messages[i].ChatId;
            vacancyIds[i] = messages[i].VacancyId;
            timestamps[i] = currentUtc;
        }

        try
        {
            var cmd = new CommandDefinition(
                INSERT_MESSAGES_SQL,
                new
                {
                    ids,
                    chat_ids = chatIds,
                    vacancy_ids = vacancyIds,
                    timestamps
                },
                cancellationToken: cancellationToken);
            var result = await connection.ExecuteAsync(cmd);
            logger.LogInformation($"Added {result}/{messages.Count} messages to database");
        }
        catch (Exception e)
        {
            throw new MessagesRepositoryException("Failed insert messages", e);
        }
    }

    private const string UPDATE_MESSAGES_SQL = 
        """
        UPDATE messages
        SET 
            chat_id = query.chat_id, 
            vacancy_id = query.vacancy_id
        FROM (
            SELECT * 
            FROM unnest(
                @ids::INT[], 
                @chat_ids::BIGINT[], 
                @vacancy_ids::BIGINT[])
            AS t(id, chat_id, vacancy_id)
        ) AS query
        WHERE messages.id = query.id;
        """;
    public async Task UpdateMessages(IList<VacancyMessage> messages, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await using var connection = connectionFactory.CreateConnection();
        var ids = new int[messages.Count];
        var chatIds = new long[messages.Count];
        var vacancyIds = new long[messages.Count];

        for (var i = 0; i < messages.Count; i++)
        {
            ids[i] = messages[i].Id;
            chatIds[i] = messages[i].ChatId;
            vacancyIds[i] = messages[i].VacancyId;
        }

        try
        {
            var cmd = new CommandDefinition(
                UPDATE_MESSAGES_SQL,
                new
                {
                    ids,
                    chat_ids = chatIds,
                    vacancy_ids = vacancyIds
                },
                cancellationToken: cancellationToken);
            var result = await connection.ExecuteAsync(cmd);
            logger.LogInformation($"Updated {result}/{messages.Count} messages in database");
        }
        catch (Exception e)
        {
            throw new MessagesRepositoryException("Failed update vacancy messages", e);
        }
    }
}