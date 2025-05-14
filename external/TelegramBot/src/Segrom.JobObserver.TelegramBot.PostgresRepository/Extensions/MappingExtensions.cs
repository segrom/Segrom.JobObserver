using Segrom.JobObserver.TelegramBot.Domain;
using Segrom.JobObserver.TelegramBot.PostgresRepository.Dao;

namespace Segrom.JobObserver.TelegramBot.PostgresRepository.Extensions;

internal static class MappingExtensions
{
	internal static VacancyMessage ToDomain(this MessageDao dao) => new()
	{
		Id = dao.Id,
		ChatId = dao.ChatId,
		VacancyId = dao.VacancyId
	};
}