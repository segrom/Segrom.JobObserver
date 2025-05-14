using System.Text.Json.Serialization;

namespace Segrom.JobObserver.OzonService.OzonJobApiClient.Dto;

public record Item(
    [property: JsonPropertyName("hhId")] long HhId,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("city")] string City,
    [property: JsonPropertyName("department")] string Department,
    [property: JsonPropertyName("employment")] string Employment,
    [property: JsonPropertyName("experience")] string Experience
);