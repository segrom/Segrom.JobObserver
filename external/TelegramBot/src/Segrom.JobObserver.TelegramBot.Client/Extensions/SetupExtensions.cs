using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Segrom.JobObserver.TelegramBot.Application.Abstractions;

namespace Segrom.JobObserver.TelegramBot.Client.Extensions;

public static class SetupExtensions
{
	public static IServiceCollection AddBotClient(this IServiceCollection services)
	{
		services.AddSingleton<IBotClient, BotClient>();
		return services;
	}

	public static IApplicationBuilder RunBotClient(this IApplicationBuilder app)
	{
		using var scope = app.ApplicationServices.CreateScope();
		var botClient = scope.ServiceProvider.GetRequiredService<IBotClient>();
		botClient.HealthCheck();
		return app;
	}
}