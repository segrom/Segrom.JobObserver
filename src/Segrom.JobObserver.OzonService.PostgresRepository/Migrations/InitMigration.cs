using FluentMigrator;

namespace Segrom.JobObserver.OzonService.PostgresRepository.Migrations;

[TimestampedMigration(2025, 5, 3, 12, 6, description: "Setup db scheme")]
public sealed class InitMigration: Migration
{
    public override void Up()
    {
        Execute.Sql(
            """
            CREATE TABLE IF NOT EXISTS vacancies (
                id BIGINT PRIMARY KEY,
                title VARCHAR(300) NOT NULL,
                city VARCHAR(300) NOT NULL,
                department VARCHAR(800) NOT NULL,
                employment VARCHAR(800) NOT NULL,
                experience VARCHAR(800) NOT NULL,
                is_closed BOOLEAN NOT NULL,
                info JSONB NULL,
                created_at TIMESTAMPTZ NOT NULL
            );
            CREATE TABLE outbox (
            	id SERIAL PRIMARY KEY,
            	topic VARCHAR(256) NOT NULL,
            	key BYTEA NULL,
            	value BYTEA NOT NULL,
            	taken_at TIMESTAMP NULL,
            	inserted_at TIMESTAMP NOT NULL
            );
            """
            );
    }

    public override void Down()
    {
        Execute.Sql(
            """
            DROP TABLE IF EXISTS vacancies;
            DROP TABLE IF EXISTS outbox;
            """
            );
    }
}