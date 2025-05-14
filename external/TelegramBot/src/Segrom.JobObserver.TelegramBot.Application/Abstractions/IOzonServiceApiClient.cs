using Segrom.JobObserver.TelegramBot.Domain;

namespace Segrom.JobObserver.TelegramBot.Application.Abstractions;

public interface IOzonServiceApiClient: IDisposable
{
	Task<IEnumerable<Vacancy>> GetVacancies(CancellationToken cancellationToken);
	Task RequestVacancyInfoFill(long vacancyId, CancellationToken cancellationToken);
}