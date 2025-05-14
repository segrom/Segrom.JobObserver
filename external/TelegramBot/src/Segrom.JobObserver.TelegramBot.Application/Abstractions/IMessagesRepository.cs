using Segrom.JobObserver.TelegramBot.Domain;

namespace Segrom.JobObserver.TelegramBot.Application.Abstractions;

public interface IMessagesRepository
{
	Task<IReadOnlyDictionary<long, List<VacancyMessage>>> GetMessagesByVacancies(IList<Vacancy> vacancies, CancellationToken cancellationToken);
	Task<IReadOnlyList<long>> GetAllChats(CancellationToken cancellationToken);
	Task AddMessages(IList<VacancyMessage> messages, CancellationToken cancellationToken);
	Task UpdateMessages(IList<VacancyMessage> messages, CancellationToken cancellationToken);
}