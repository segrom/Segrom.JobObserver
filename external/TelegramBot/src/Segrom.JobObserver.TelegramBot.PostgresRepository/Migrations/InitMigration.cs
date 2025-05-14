using FluentMigrator;

namespace Segrom.JobObserver.TelegramBot.PostgresRepository.Migrations;

[TimestampedMigration(2025, 5, 10, 15, 25, description: "Setup db scheme")]
public sealed class InitMigration: Migration
{
    public override void Up()
    {
        Execute.Sql(
            """
            CREATE TABLE IF NOT EXISTS messages (
                id INT NOT NULL,
                chat_id BIGINT NOT NULL,
                vacancy_id BIGINT NOT NULL,
                created_at TIMESTAMPTZ NOT NULL,
                CONSTRAINT pk_message PRIMARY KEY (id, chat_id)
            );
            """
            );
    }

    public override void Down()
    {
        Execute.Sql(
            """
            DROP TABLE IF EXISTS messages;
            """
            );
    }
}