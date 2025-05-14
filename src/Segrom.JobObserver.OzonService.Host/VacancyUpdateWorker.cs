using Segrom.JobObserver.OzonService.Application.Abstractions;

namespace Segrom.JobObserver.OzonService.Host;

internal sealed class VacancyUpdateWorker(
	ILogger<VacancyUpdateWorker> logger,
	IConfiguration configuration,
	IServiceProvider serviceProvider
	): BackgroundService
{
	private int _period;

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		_period = configuration.GetValue<int>("VacancyUpdatePeriod");
		await Task.Yield();
		while (!stoppingToken.IsCancellationRequested)
		{
			logger.LogInformation("VacancyUpdateWorker start running at: {time}", DateTimeOffset.Now);
			try
			{
				await Run(stoppingToken);
			}
			catch (Exception e)
			{
				logger.LogError(e, "Error while update vacancies: {Message}, {Stacktrace}", e.Message, e.StackTrace);
				await Task.Delay(1000, stoppingToken);
			}
		}
	}
	
	private async Task Run(CancellationToken cancellationToken)
	{
		while (!cancellationToken.IsCancellationRequested)
		{
			await using var scope = serviceProvider.CreateAsyncScope();
			var service = scope.ServiceProvider.GetRequiredService<IOzonVacancyService>();
			
			await service.UpdateVacancies(cancellationToken);
			
			await Task.Delay(TimeSpan.FromSeconds(_period), cancellationToken);
		}
	}
}