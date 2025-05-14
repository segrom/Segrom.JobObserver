using Google.Protobuf.Collections;
using Grpc.Core;
using Segrom.JobObserver.OzonService.Application.Abstractions;
using Segrom.JobObserver.OzonService.Grpc;

namespace Segrom.JobObserver.OzonService.Host.Grpc;

public class GrpcOzonService(IOzonVacancyService service, ILogger<GrpcOzonService> logger) : OzonService.Grpc.OzonService.OzonServiceBase
{
    public override async Task<VacanciesResponse> GetVacancies(VacanciesRequest request, ServerCallContext context)
    {
        try
        {
            var result = await service.GetVacanciesAsync(request.Limit, request.Page ,context.CancellationToken); // todo: add validations
            var response = new VacanciesResponse
            {
                Success = new VacanciesResponse.Types.Ok()
            };
            response.Success.Vacancies.AddRange(result.Select(x=> x.ToGrpc()));
            return response;
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return new VacanciesResponse
            {
                Error = new Error
                {
                    Code = "Force update failed",
                    Message = e.Message
                }
            };
        }
    }

    public override async Task<StatusResponse> ForceUpdate(VoidRequest request, ServerCallContext context)
    {
        try
        {
            await service.UpdateVacancies(context.CancellationToken);
            return new StatusResponse();
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return new StatusResponse
            {
                Error = new Error
                {
                    Code = "Force update failed",
                    Message = e.Message
                }
            };
        }
    }

    public override async Task<StatusResponse> FillVacancyInfo(FillVacancyInfoRequest request, ServerCallContext context)
    {
        try
        {
            logger.LogInformation("Vacancy ({Id}) info fill request", request.VacancyId);
            await service.FillVacancyInfo(request.VacancyId, context.CancellationToken);
            return new StatusResponse();
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return new StatusResponse
            {
                Error = new Error
                {
                    Code = "Force update failed",
                    Message = e.Message
                }
            };
        }
    }
}