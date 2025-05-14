using System.Text.Json.Serialization;

namespace Segrom.JobObserver.OzonService.OzonJobApiClient.Dto;

public record Meta(
    [property: JsonPropertyName("limit")] int? Limit,
    [property: JsonPropertyName("page")] int? Page,
    [property: JsonPropertyName("perPage")] int? PerPage,
    [property: JsonPropertyName("totalItems")] int? TotalItems,
    [property: JsonPropertyName("totalPages")] int? TotalPages
);