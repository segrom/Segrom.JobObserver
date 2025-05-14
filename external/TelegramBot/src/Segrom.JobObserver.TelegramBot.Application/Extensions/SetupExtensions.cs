using Microsoft.Extensions.DependencyInjection;
using Segrom.JobObserver.TelegramBot.Application.Abstractions;

namespace Segrom.JobObserver.TelegramBot.Application.Extensions;

public static class SetupExtensions
{
	public static IServiceCollection AddApplication(this IServiceCollection services)
	{
		services.AddTransient<ITimeService, TimeService>();
		services.AddTransient<IBotService, BotService>();
		return services;
	}
}