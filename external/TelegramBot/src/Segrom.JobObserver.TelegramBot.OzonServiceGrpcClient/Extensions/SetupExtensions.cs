using Microsoft.Extensions.DependencyInjection;
using Segrom.JobObserver.TelegramBot.Application.Abstractions;

namespace Segrom.JobObserver.TelegramBot.OzonServiceGrpcClient.Extensions;

public static class SetupExtensions
{
	public static IServiceCollection AddOzonVacancyApiClient(this IServiceCollection services)
	{
		services.AddTransient<IOzonServiceApiClient, OzonServiceGrpcClient>();
		return services;
	}
}