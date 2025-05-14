using System.Text.Json;
using Segrom.JobObserver.OzonService.Grpc;

namespace Segrom.JobObserver.OzonService.Host.Grpc;

public static class MapExtensions
{
	public static Vacancy ToGrpc(this Domain.Vacancy m) => new()
	{
		Id = m.Id,
		City = m.City,
		Department = m.Department,
		Employment = m.Employment,
		Experience = m.Experience,
		Title = m.Title,
		Info = JsonSerializer.Serialize(m.Info),
		IsClosed = m.IsClosed
	};
}