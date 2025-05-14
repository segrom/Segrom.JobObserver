using Segrom.JobObserver.TelegramBot.Domain;

namespace Segrom.JobObserver.TelegramBot.Application.Abstractions;

public interface IBotService
{
	Task HandleNewVacancies(List<Vacancy> vacancies, CancellationToken cancellationToken);
	Task HandleUpdateVacancies(List<Vacancy> vacancies, CancellationToken cancellationToken);
	Task InitNewChat(long chatId, CancellationToken cancellationToken);
	Task RequestVacancyInfoFill(long vacancyId, CancellationToken cancellationToken);
}