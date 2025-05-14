using Segrom.JobObserver.TelegramBot.Domain;

namespace Segrom.JobObserver.TelegramBot.Application.Abstractions;

public interface IBotClient: IDisposable
{
	Task UpdateMessageWithVacancy(Vacancy vacancy, VacancyMessage message, CancellationToken cancellationToken);
	Task<int> SendNewVacancyMessage(Vacancy vacancy, long chatId, CancellationToken cancellationToken);
	void HealthCheck();
}