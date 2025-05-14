using System.Text.Json.Serialization;

namespace Segrom.JobObserver.OzonService.OzonJobApiClient.Dto;

public record VacanciesDto(
    [property: JsonPropertyName("items")] IReadOnlyList<Item> Items,
    [property: JsonPropertyName("meta")] Meta Meta
);



