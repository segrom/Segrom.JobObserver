using Microsoft.Extensions.DependencyInjection;
using Segrom.JobObserver.TelegramBot.Application.Abstractions;
using Segrom.JobObserver.TelegramBot.PostgresRepository.Abstractions;
using Segrom.JobObserver.TelegramBot.PostgresRepository.Factories;

namespace Segrom.JobObserver.TelegramBot.PostgresRepository.Extensions;

public static class SetupExtensions
{
    public static IServiceCollection AddMessagesRepository(this IServiceCollection services)
    {
        services.AddSingleton<IPostgresConnectionFactory, PostgresConnectionFactory>();
        services.AddTransient<IMessagesRepository, MessagesRepository>();
        return services;
    }
}