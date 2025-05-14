using Microsoft.Extensions.Logging;
using Segrom.JobObserver.TelegramBot.Application.Abstractions;
using Segrom.JobObserver.TelegramBot.Application.Exceptions;
using Segrom.JobObserver.TelegramBot.Domain;

namespace Segrom.JobObserver.TelegramBot.Application;

internal sealed class BotService(
	IMessagesRepository repository, 
	IBotClient bot,
	IOzonServiceApiClient vacanciesApi,
	ILogger<BotService> logger
	): IBotService
{
	public async Task HandleNewVacancies(List<Vacancy> vacancies, CancellationToken cancellationToken)
	{
		try
		{
			var chats = await repository.GetAllChats(cancellationToken);
			var newMessages = new List<VacancyMessage>();
			
			foreach (var chatId in chats)
			{
				foreach (var vacancy in vacancies)
				{
					var messageId = await bot.SendNewVacancyMessage(vacancy, chatId, cancellationToken);
					newMessages.Add(new VacancyMessage
					{
						Id = messageId,
						ChatId = chatId,
						VacancyId = vacancy.Id
					});
				}
			}
			
			await repository.AddMessages(newMessages, cancellationToken);
		}
		catch (Exception e)
		{
			//logger.LogError(e, "Failed to handle new vacancies, error: {Error}", e.Message);
			throw new AppException("Failed to handle new vacancies", e);
		}
	}

	public async Task HandleUpdateVacancies(List<Vacancy> vacancies, CancellationToken cancellationToken)
	{
		try
		{
			var messagesPerVacancies = await repository.GetMessagesByVacancies(vacancies, cancellationToken);
			logger.LogInformation("Received {MessagesCount} messages for {VacanciesCount} vacancies", messagesPerVacancies.Count, vacancies.Count);
			foreach (var vacancy in vacancies)
			{
				var messages = messagesPerVacancies[vacancy.Id];
				foreach (var message in messages)
				{
					await bot.UpdateMessageWithVacancy(vacancy, message, cancellationToken);
				}
			}
		}
		catch (Exception e)
		{
			//logger.LogError(e, "Failed to update vacancies, error: {Error}", e.Message);
			throw new AppException("Failed to update vacancies", e);
		}
	}

	public async Task InitNewChat(long chatId, CancellationToken cancellationToken)
	{
		try
		{
			var vacancies = await vacanciesApi.GetVacancies(cancellationToken);
			var newMessages = new List<VacancyMessage>();

			foreach (var vacancy in vacancies)
			{
				var messageId = await bot.SendNewVacancyMessage(vacancy, chatId, cancellationToken);
				newMessages.Add(new VacancyMessage
				{
					Id = messageId,
					ChatId = chatId,
					VacancyId = vacancy.Id
				});
			}
			await repository.AddMessages(newMessages, cancellationToken);
		}
		catch (Exception e)
		{
			//logger.LogError(e, "Failed to init new chat, error: {Message}", e.Message);
			throw new AppException("Failed to init new chat", e);
		}
	}

	public async Task RequestVacancyInfoFill(long vacancyId, CancellationToken cancellationToken)
	{
		try
		{
			await vacanciesApi.RequestVacancyInfoFill(vacancyId ,cancellationToken);
		}
		catch (Exception e)
		{
			//logger.LogError(e, "Failed to request vacancy info fill, error: {Error}", e.Message);
			throw new AppException("Failed to request vacancy info fill", e);
		}
	}
}