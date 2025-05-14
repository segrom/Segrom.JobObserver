using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Segrom.JobObserver.TelegramBot.Domain;
using Segrom.JobObserver.TelegramBot.KafkaConsumer.Abstractions;
using Segrom.JobObserver.TelegramBot.KafkaConsumer.Factories;

namespace Segrom.JobObserver.TelegramBot.KafkaConsumer.Extensions;

public static class SetupExtensions
{
	public static IServiceCollection AddKafkaConsumers(this IServiceCollection services)
	{
		services.AddTransient<IDeserializer<List<Vacancy>>, JsonDeserializer<List<Vacancy>>>();
		services.AddSingleton<IConsumerFactory<string, List<Vacancy>>, ConsumerFactory<string, List<Vacancy>>>();
		services.AddHostedService<VacancyNewConsumer>();
		services.AddHostedService<VacancyUpdateConsumer>();
		return services;
	}
}