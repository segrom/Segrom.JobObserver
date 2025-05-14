namespace Segrom.JobObserver.OzonService.Domain;

public sealed class Vacancy
{
    public long Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public string Employment { get; init; } = string.Empty;
    public string Experience { get; init; } = string.Empty;
    
    public bool IsClosed { get; set; }
    
    public VacancyInfo? Info { get; set; } = null;
    
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Title, City, Department, Employment, Experience, IsClosed, Info);
    }
}