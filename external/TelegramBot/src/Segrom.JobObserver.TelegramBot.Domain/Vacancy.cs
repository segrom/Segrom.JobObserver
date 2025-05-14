namespace Segrom.JobObserver.TelegramBot.Domain;

public sealed class Vacancy
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Employment { get; set; } = string.Empty;
    public string Experience { get; set; } = string.Empty;
    public bool IsClosed { get; set; }
    
    public VacancyInfo? Info { get; set; } = null;
    
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}