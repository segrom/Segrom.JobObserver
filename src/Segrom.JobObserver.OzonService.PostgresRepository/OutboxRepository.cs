using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using Segrom.JobObserver.OzonService.Application;
using Segrom.JobObserver.OzonService.Application.Abstractions;
using Segrom.JobObserver.OzonService.Application.Exceptions;
using Segrom.JobObserver.OzonService.Application.Options;
using Segrom.JobObserver.OzonService.PostgresRepository.Abstractions;
using Segrom.JobObserver.OzonService.PostgresRepository.Dao;

namespace Segrom.JobObserver.OzonService.PostgresRepository;

internal sealed class OutboxRepository(
    ITimeService timeService,
    IOptions<OutboxOptions> options,
    IPostgresConnectionFactory connectionFactory,
    ILogger<VacancyRepository> logger): IOutboxFiller, IOutboxRepository
{
    private const string INSERT_INTO_OUTBOX_SQL = 
        """
        INSERT INTO outbox VALUES (DEFAULT, @topic, @key, @value, NULL, @inserted_at);
        """;
    public async Task InsertRecord(
        NpgsqlTransaction transaction, 
        string topic, byte[] value, byte[]? key = null, 
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            var connection = transaction.Connection!;
            var rows = await connection.ExecuteAsync(INSERT_INTO_OUTBOX_SQL, new
            {
                topic,
                key,
                value,
                inserted_at = timeService.GetCurrentUtcTime(),
            }, transaction);
            logger.LogInformation("Was insert into outbox affected rows: {Rows}", rows);
        }
        catch (Exception e)
        {
            throw new OutboxRepositoryException($"Failed insert records: {e.Message}", e);
        }
    }

    private const string GET_WITH_UPDATE_OUTBOX_RECORDS_SQL = 
        """
        WITH locked_records AS (
            SELECT id, topic, key, value
            FROM outbox
            WHERE (taken_at IS NULL OR taken_at < @taken_lifetime)
            FOR UPDATE SKIP LOCKED
            LIMIT @limit
        ), 
        updated AS (
            UPDATE outbox
            SET taken_at = @taken_at
            WHERE id IN (SELECT id FROM locked_records)
        )
        SELECT id AS Id, topic AS Topic, key AS Key, value AS Value
        FROM locked_records;
        """;
    public async Task<List<OutboxRecord>> GetRecords(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            await using var connection = connectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);
            var records = await connection.QueryAsync<OutboxRecordDao>(
                GET_WITH_UPDATE_OUTBOX_RECORDS_SQL, new
                {
                    limit = options.Value.BachSize,
                    taken_at = timeService.GetCurrentUtcTime(),
                    taken_lifetime = timeService.GetCurrentUtcTime().AddMilliseconds(-options.Value.TakenLifetime)
                });
            var recordsList = records.Select(x => new OutboxRecord(x.Id, x.Topic, x.Value, x.Key)).ToList();
			
            if (recordsList.Count <= 0)
                return [];
			
            logger.LogInformation("Found {recordsCount} records: {records}", recordsList.Count, string.Join(',', recordsList.Select(r => $"'{r.Id}'")));
            return recordsList;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed get outbox records: {Message} (transaction rollback success)", e.Message);
            throw new OutboxRepositoryException("Failed to get outbox records", e);
        }
    }

    private const string DELETE_OUTBOX_RECORDS_SQL =
        """DELETE FROM outbox WHERE id = ANY(@deleteIds::INTEGER[]);""";
    public async Task DeleteRecords(List<OutboxRecord> records, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await using var connection = connectionFactory.CreateConnection();
        var ids = records.Select(x => x.Id).ToArray();
        try
        {
            await connection.ExecuteAsync(DELETE_OUTBOX_RECORDS_SQL, new
            {
                deleteIds = ids,
                cancellationToken
            });
        }
        catch (Exception e)
        {
            logger.LogError(e, 
                "Failed delete records: {Message}; Records: {RecordsIds}", 
                e.Message, string.Join(", ", ids));
            throw new OutboxRepositoryException("Failed delete records", e);
        }
    }
}