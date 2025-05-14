namespace Segrom.JobObserver.OzonService.PostgresRepository.Dao;

internal sealed class OutboxRecordDao
{
		public int Id { get; set; }
		public string Topic { get; set; } = string.Empty;
		public byte[] Key { get; set; } = [];
		public byte[] Value { get; set; } = [];
		public DateTime TakenAt { get; set; }
		public DateTime InsertedAt { get; set; }
}