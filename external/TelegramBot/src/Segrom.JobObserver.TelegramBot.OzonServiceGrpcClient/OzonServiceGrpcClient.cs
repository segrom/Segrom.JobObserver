using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Segrom.JobObserver.OzonService.Grpc;
using Segrom.JobObserver.TelegramBot.Application.Abstractions;
using Segrom.JobObserver.TelegramBot.Application.Exceptions;
using Vacancy = Segrom.JobObserver.TelegramBot.Domain.Vacancy;

namespace Segrom.JobObserver.TelegramBot.OzonServiceGrpcClient;

internal sealed class OzonServiceGrpcClient: IOzonServiceApiClient
{
	private readonly OzonService.Grpc.OzonService.OzonServiceClient _client;
	private readonly GrpcChannel _ordersChannel;

	public OzonServiceGrpcClient(IConfiguration configuration)
	{
		var url = configuration.GetSection("OZON_VACANCY_SERVICE_URL").Value;
		if (string.IsNullOrEmpty(url)) 
			throw new OzonServiceApiException("OZON_VACANCY_SERVICE_URL must be specified");
		
		_ordersChannel = GrpcChannel.ForAddress(url);
		_client = new OzonService.Grpc.OzonService.OzonServiceClient(_ordersChannel);
	}

	public async Task<IEnumerable<Vacancy>> GetVacancies(CancellationToken cancellationToken)
	{
		var result = await _client.GetVacanciesAsync(new VacanciesRequest
		{
			Page = 0,
			Limit = 1024
		}, new CallOptions(cancellationToken: cancellationToken));
		
		if (result is null) 
			throw new OzonServiceApiException("GetVacancies failed");
		if (result.ResultCase == VacanciesResponse.ResultOneofCase.Error) 
			throw new OzonServiceApiException($"Get vacancies failed: [{result.Error.Code}] {result.Error.Message}]");
		
		return result.Success.Vacancies.Select(ToDomain);
	}

	public async Task RequestVacancyInfoFill(long vacancyId, CancellationToken cancellationToken)
	{
		var result = await _client.FillVacancyInfoAsync(new FillVacancyInfoRequest
		{
			VacancyId = vacancyId,
		}, new CallOptions(cancellationToken: cancellationToken));
		
		if (result is null) 
			throw new OzonServiceApiException("RequestVacancyInfoFill failed");
		if (result.Error is { } error) 
			throw new OzonServiceApiException($"RequestVacancyInfoFill failed: [{error.Code}] {error.Message}]");
	}

	private Vacancy ToDomain(OzonService.Grpc.Vacancy dto) => new()
	{
		Id = dto.Id,
		Title = dto.Title,
		City = dto.City,
		Department = dto.Department,
		Employment = dto.Employment,
		Experience = dto.Experience,
		IsClosed = dto.IsClosed
	};

	public void Dispose()
	{
		_ordersChannel.Dispose();
	}
}