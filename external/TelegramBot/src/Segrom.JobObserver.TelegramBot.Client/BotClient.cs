using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prometheus;
using Segrom.JobObserver.TelegramBot.Application.Abstractions;
using Segrom.JobObserver.TelegramBot.Application.Exceptions;
using Segrom.JobObserver.TelegramBot.Client.Messages;
using Segrom.JobObserver.TelegramBot.Domain;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Segrom.JobObserver.TelegramBot.Client;

internal sealed class BotClient: IBotClient
{
	private readonly TelegramBotClient _bot;
	private readonly IServiceProvider _serviceProvider;
	private readonly ILogger<BotClient> _logger;
	
	private static readonly Counter NewMessagesCounter = Metrics.CreateCounter(
		"bot_new_messages_total",
		"Total number of new messages",
		new CounterConfiguration
		{
			LabelNames = ["status", "category"]
		});

	private static readonly Counter UpdateMessagesCounter = Metrics.CreateCounter(
		"bot_update_messages_total",
		"Total number of update messages",
		new CounterConfiguration
		{
			LabelNames = ["status", "category"]
		});
	
	private static readonly Counter BotCommandsCounter = Metrics.CreateCounter(
		"bot_commands_total",
		"Total number of bot commands",
		new CounterConfiguration
		{
			LabelNames = ["command", "category"]
		});
	
	public BotClient(IConfiguration configuration, IServiceProvider serviceProvider, ILogger<BotClient> logger)
	{
		_logger = logger;
		_serviceProvider = serviceProvider;
		var token = configuration.GetSection("TG_BOT_TOKEN").Value 
		            ?? throw new BotClientException("Bot token is missing from configuration");
		
		_bot = new TelegramBotClient(token);
		_bot.OnError += HandleError;
		_bot.OnMessage += HandleMessage;
		_bot.OnUpdate += HandleUpdate;
	}

	public void HealthCheck()
	{
		_logger.LogInformation("Bot Healthy! ({Id})", _bot.BotId);
	}

	public async Task<int> SendNewVacancyMessage(Vacancy vacancy, long chatId, CancellationToken cancellationToken)
	{
		try
		{
			var serializedMessage = new VacancyHtmlMessage(vacancy);

			var message = await _bot.SendMessage(
				new ChatId(chatId),
				serializedMessage.ToString(),
				ParseMode.Html,
				replyMarkup: serializedMessage.GetMarkup(),
				cancellationToken: cancellationToken
			);
			_logger.LogInformation("Sent vacancy data: {Id}", vacancy.Id);
			NewMessagesCounter.WithLabels("success","bot").Inc();
			return message.Id;
		}
		catch (Exception e)
		{
			NewMessagesCounter.WithLabels("failed","bot").Inc();
			try
			{
				await _bot.SendMessage(
					new ChatId(chatId),
					$"🚫 Не удалось отправить сообщение по вакансии: {vacancy.Title} ({vacancy.Id})",
					cancellationToken: cancellationToken);
			}
			catch (Exception exception)
			{
				_logger.LogError(exception, "Error while trying modify message error");
			}
			
			throw new BotClientException($"Error while sending new vacancy data: {vacancy.Id}, chatId: {chatId}", e);
		}
	}

	public async Task UpdateMessageWithVacancy(Vacancy vacancy, VacancyMessage message,
		CancellationToken cancellationToken)
	{
		try
		{
			var serializedMessage = new VacancyHtmlMessage(vacancy);

			await _bot.EditMessageText(
				new ChatId(message.ChatId),
				message.Id,
				serializedMessage.ToString(),
				ParseMode.Html,
				replyMarkup: serializedMessage.GetMarkup(),
				cancellationToken: cancellationToken);
			UpdateMessagesCounter.WithLabels("success","bot").Inc();
			_logger.LogInformation("Updated vacancy data: {Id}, message: {MessageId}, chat: {ChatId}",
				vacancy.Id, message.Id, message.ChatId);
		}
		catch (Exception e)
		{
			UpdateMessagesCounter.WithLabels("failed","bot").Inc();
			try
			{
				await _bot.SetMessageReaction(
					new ChatId(message.ChatId),
					message.Id,
					[ new ReactionTypeEmoji{Emoji = "😢"}],
					true,
					cancellationToken: cancellationToken);
			}
			catch (Exception exception)
			{
				_logger.LogError(exception, "Error while trying modify message error");
			}
			throw new BotClientException(
				$"Error while sending update vacancy data: {vacancy.Id}, message: {JsonSerializer.Serialize(message)}", 
				e);
		}
	}
	
	private async Task HandleMessage(Message message, UpdateType type)
	{
		try
		{
			using var scope = _serviceProvider.CreateScope();
			var service = scope.ServiceProvider.GetService<IBotService>()!;

			if (message.Text == "/start")
			{
				BotCommandsCounter.WithLabels("start", "bot").Inc();
				await _bot.SendMessage(message.Chat, 
					"Привет! Этот бот поможет следить за новыми вакансиями.\nВот актуальные вакансии на данный момент:",
					ParseMode.Html);
				await service.InitNewChat(message.Chat.Id, _bot.GlobalCancelToken);
				await _bot.SendMessage(message.Chat,
					"Вы подписались на обновления!\nЧтобы отписаться отправьте /unsubscribe",
					ParseMode.Html);
				return;
			}

			if (message.Text == "/unsubscribe")
			{
				BotCommandsCounter.WithLabels("unsubscribe", "bot").Inc();
				await _bot.SendMessage(message.Chat, "Извините, пока отписка не работает 😅", ParseMode.Html);
				// todo: unsubscribe functionality
				//await _bot.SendMessage(message.Chat, "Вы успешно отписались!", ParseMode.Html);
				return;
			}

			await _bot.DeleteMessage(message.Chat, message.MessageId);
		}
		catch (Exception e)
		{
			_logger.LogError(e, "Failed handle message: {Message}, error: {ErrorMessage}", message.Text, e.Message);
		}
	}

	private Task HandleError(Exception exception, HandleErrorSource source)
	{
		_logger.LogError(exception, "Handle error: {Message}, source {Source}",
			exception.Message,
			source);
		return Task.CompletedTask;
	}
	
	private async Task HandleUpdate(Update update)
	{
		using var scope = _serviceProvider.CreateScope();
		var service = scope.ServiceProvider.GetService<IBotService>()!;
		
		if (update is { Type: UpdateType.CallbackQuery, CallbackQuery: { } callbackQuery })
		{
			var parts = callbackQuery.Data?.Split(':');
			if (parts is null)
			{
				_logger.LogWarning("Callback query data could not be parsed: \'{QueryData}\'", callbackQuery.Data);
				return;
			}
			if (parts.First().Equals("fill_info"))
			{
				if (!long.TryParse(parts.Last(), out var vacancyId))
				{
					_logger.LogError("Failed to parse vacancy id in CallbackQuery: {Id}", callbackQuery.Data);
					return;
				}
				BotCommandsCounter.WithLabels("fill_request", "bot").Inc();
				await service.RequestVacancyInfoFill(vacancyId, _bot.GlobalCancelToken);
				await _bot.AnswerCallbackQuery(callbackQuery.Id, "Уточняем информацию по вакансии");
				if (callbackQuery.Message is { } message)
				{
					await _bot.EditMessageReplyMarkup(message.Chat.Id, message.MessageId, null);
				}
			}
		}
			
	}

	public void Dispose()
	{
		_bot.OnError -= HandleError;
		_bot.OnMessage -= HandleMessage;
		_bot.OnUpdate -= HandleUpdate;
	}

}