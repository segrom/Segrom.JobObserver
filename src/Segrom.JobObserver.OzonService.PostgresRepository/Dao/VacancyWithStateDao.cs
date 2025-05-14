using Segrom.JobObserver.OzonService.Domain;

namespace Segrom.JobObserver.OzonService.PostgresRepository.Dao;

internal sealed class VacancyWithStateDao
{
	public long Id { get; set; }
	public string Title { get; set; } = string.Empty;
	public string City { get; set; } = string.Empty;
	public string Department { get; set; } = string.Empty;
	public string Employment { get; set; } = string.Empty;
	public string Experience { get; set; } = string.Empty;
	public string? Info { get; set; } = null;
	public bool IsClosed { get; set; }
	public DateTimeOffset CreatedAt { get; set; }
}