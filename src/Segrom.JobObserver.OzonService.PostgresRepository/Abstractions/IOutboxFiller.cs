using System.Transactions;
using Npgsql;

namespace Segrom.JobObserver.OzonService.PostgresRepository.Abstractions;

internal interface IOutboxFiller
{
	Task InsertRecord(NpgsqlTransaction transaction, string topic,  byte[] value, byte[]? key = null);
}