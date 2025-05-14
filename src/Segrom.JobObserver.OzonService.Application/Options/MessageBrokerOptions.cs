namespace Segrom.JobObserver.OzonService.Application.Options;

public sealed class MessageBrokerOptions
{
	public string NewVacanciesTopic { get; set; } = string.Empty;
	public string UpdateVacanciesTopic { get; set; } = string.Empty;
}