using System.Text;
using System.Text.Json;
using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Segrom.JobObserver.OzonService.Application;
using Segrom.JobObserver.OzonService.Application.Abstractions;
using Segrom.JobObserver.OzonService.Application.Exceptions;
using Segrom.JobObserver.OzonService.Application.Options;
using Segrom.JobObserver.OzonService.Domain;
using Segrom.JobObserver.OzonService.PostgresRepository.Abstractions;
using Segrom.JobObserver.OzonService.PostgresRepository.Dao;

namespace Segrom.JobObserver.OzonService.PostgresRepository;

internal sealed class VacancyRepository(
    IOptions<MessageBrokerOptions> brokerOptions,
    IPostgresConnectionFactory connectionFactory,
    IOutboxFiller outboxFiller,
    ILogger<VacancyRepository> logger): IVacancyRepository
{
    private const string INSERT_INTO_VACANCIES_SQL = 
        """
        INSERT INTO vacancies  
        (id, title, city, department, employment, experience, is_closed, info, created_at)
        SELECT * 
        FROM unnest(
          @ids::BIGINT[], 
          @titles::VARCHAR(300)[], 
          @cities::VARCHAR(300)[],
          @departments::VARCHAR(800)[],
          @employments::VARCHAR(800)[],
          @experiences::VARCHAR(800)[],
          @close_statuses::BOOLEAN[],
          @infos::JSONB[],
          @timestamps::TIMESTAMPTZ[]);
        """;
    public async Task CreateVacanciesAsync(IReadOnlyList<Vacancy> vacancies, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        
        var ids = new long[vacancies.Count];
        var titles = new string[vacancies.Count];
        var cities = new string[vacancies.Count];
        var departments = new string[vacancies.Count];
        var employments = new string[vacancies.Count];
        var experiences = new string[vacancies.Count];
        var closeStatuses = new bool[vacancies.Count];
        var infos = new string?[vacancies.Count];
        var timestamps = new DateTimeOffset[vacancies.Count];

        for (var i = 0; i < vacancies.Count; i++)
        {
            ids[i] = vacancies[i].Id;
            titles[i] = vacancies[i].Title;
            cities[i] = vacancies[i].City;
            departments[i] = vacancies[i].Department;
            employments[i] = vacancies[i].Employment;
            experiences[i] = vacancies[i].Experience;
            closeStatuses[i] = vacancies[i].IsClosed;
            timestamps[i] = vacancies[i].CreatedAt;
            infos[i] = vacancies[i].Info is null 
                ? null
                : JsonSerializer.Serialize(vacancies[i].Info);
        }
        
        try
        {
            var cmd = new CommandDefinition(
                INSERT_INTO_VACANCIES_SQL, 
                new
                {
                    ids, titles, cities, departments, employments, experiences, timestamps, infos,
                    close_statuses = closeStatuses
                },
                transaction: transaction,
                cancellationToken: cancellationToken);
            
            await connection.ExecuteAsync(cmd);
            
            await outboxFiller.InsertRecord(
                transaction, 
                topic: brokerOptions.Value.NewVacanciesTopic,
                value: Encoding.Unicode.GetBytes(JsonSerializer.Serialize(vacancies)), 
                cancellationToken: cancellationToken);
            
            await transaction.CommitAsync(cancellationToken);
            logger.LogInformation("Created {Count} vacancies", vacancies.Count);
        }
        catch (Exception e)
        {
            throw new VacancyRepositoryException("Failed insert vacancies", e);
        }
    }
    
        private const string UPDATE_VACANCIES_SQL = 
        """
        UPDATE vacancies
        SET 
            title = query.title, 
            city = query.city, 
            department = query.department, 
            employment = query.employment, 
            experience = query.experience, 
            info = query.info,
            is_closed = query.is_closed
        FROM (
            SELECT * 
            FROM unnest(
            @ids::BIGINT[], 
            @titles::VARCHAR(300)[], 
            @cities::VARCHAR(300)[],
            @departments::VARCHAR(800)[],
            @employments::VARCHAR(800)[],
            @experiences::VARCHAR(800)[],
            @infos::JSONB[],
            @close_statuses::BOOLEAN[])
            AS t(id, title, city, department, employment, experience, info, is_closed)
        ) AS query
        WHERE vacancies.id = query.id;
        """;
    public async Task UpdateVacanciesAsync(IReadOnlyList<Vacancy> vacancies, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        
        var ids = new long[vacancies.Count];
        var titles = new string[vacancies.Count];
        var cities = new string[vacancies.Count];
        var departments = new string[vacancies.Count];
        var employments = new string[vacancies.Count];
        var experiences = new string[vacancies.Count];
        var infos = new string?[vacancies.Count];
        var closeStatuses = new bool[vacancies.Count];

        for (var i = 0; i < vacancies.Count; i++)
        {
            ids[i] = vacancies[i].Id;
            titles[i] = vacancies[i].Title;
            cities[i] = vacancies[i].City;
            departments[i] = vacancies[i].Department;
            employments[i] = vacancies[i].Employment;
            experiences[i] = vacancies[i].Experience;
            closeStatuses[i] = vacancies[i].IsClosed;
            infos[i] = vacancies[i].Info is null 
                ? null
                : JsonSerializer.Serialize(vacancies[i].Info);
        }
        
        try
        {
            var cmd = new CommandDefinition(
                UPDATE_VACANCIES_SQL, 
                new
                {
                    ids, titles, cities, departments, employments, experiences, infos,
                    close_statuses = closeStatuses
                },
                cancellationToken: cancellationToken);
            await connection.ExecuteAsync(cmd);
            
            await outboxFiller.InsertRecord(transaction,
                brokerOptions.Value.UpdateVacanciesTopic,
                Encoding.Unicode.GetBytes(JsonSerializer.Serialize(vacancies)));
            
            await transaction.CommitAsync(cancellationToken);
            logger.LogInformation("Updated {Count} vacancies", vacancies.Count);
        }
        catch (Exception e)
        {
            throw new VacancyRepositoryException("Failed update vacancies", e);
        }
    }
    
    private const string VACANCIES_REQUEST_SQL = 
        """
        SELECT 
            id AS Id, 
            title AS Title, 
            city AS City, 
            department AS Department, 
            employment AS Employment, 
            experience AS Experience, 
            is_closed AS IsClosed, 
            info AS Info, 
            created_at AS CreatedAt
        FROM vacancies
        __QUERY__
        ORDER BY id
        OFFSET @offset
        LIMIT @limit;
        """;
    public async Task<IReadOnlyList<Vacancy>> GetVacancies(VacanciesQuery query, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            var sql = VACANCIES_REQUEST_SQL;
            var conditions = new List<string>();

            if (query.Ids is not null)
            {
                conditions.Add("id = ANY(@ids::BIGINT[])");
            }
            if (query.Cities is not null)
            {
                conditions.Add("city = ANY(@cities::VARCHAR(300)[])");
            }
            if (query.IsClosed is not null)
            {
                conditions.Add("is_closed = @is_closed");
            }
            
            var whereQuery = conditions.Count > 0 
                ? $"WHERE {string.Join(" AND ", conditions)}" 
                : "";
            sql = sql.Replace("__QUERY__", whereQuery);
            
            await using var connection = connectionFactory.CreateConnection();
            var cmd = new CommandDefinition(
                sql, 
                new { 
                    offset = query.Offset, 
                    limit = query.Limit,
                    ids = query.Ids,
                    cities = query.Cities,
                    is_closed = query.IsClosed,
                }, 
                cancellationToken: cancellationToken);
            
            var operations = await connection.QueryAsync<VacancyWithStateDao>(cmd);
            return operations.Select(x => new Vacancy
            {
                Id = x.Id,
                Title = x.Title,
                City = x.City,
                Department = x.Department,
                Employment = x.Employment,
                Experience = x.Experience,
                IsClosed = x.IsClosed,
                CreatedAt = x.CreatedAt,
                Info = x.Info is null 
                    ? null 
                    : JsonSerializer.Deserialize<VacancyInfo>(x.Info),
            }).AsList();
        }
        catch (Exception e)
        {
            throw new VacancyRepositoryException($"Failed to get vacancies by query {JsonSerializer.Serialize(query)}", e);
        }
    }
}