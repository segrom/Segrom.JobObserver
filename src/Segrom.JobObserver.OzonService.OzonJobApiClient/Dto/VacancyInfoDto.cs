using System.Text.Json.Serialization;

namespace Segrom.JobObserver.OzonService.OzonJobApiClient.Dto;

public record Address(
	[property: JsonPropertyName("street")] string Street,
	[property: JsonPropertyName("lat")] double? Lat,
	[property: JsonPropertyName("lng")] double? Lng,
	[property: JsonPropertyName("metro")] Metro Metro
);

public record Country(
	[property: JsonPropertyName("ID")] int? Id,
	[property: JsonPropertyName("title")] string Title
);

public record Manager(
	[property: JsonPropertyName("login")] string Login
);

public record VacancyInfoDto(
	[property: JsonPropertyName("name")] string Name,
	[property: JsonPropertyName("dep")] string Dep,
	[property: JsonPropertyName("city")] string City,
	[property: JsonPropertyName("descr")] string Description,
	[property: JsonPropertyName("exp")] string Exp,
	[property: JsonPropertyName("employment")] string Employment,
	[property: JsonPropertyName("rubrics")] IReadOnlyList<string> Rubrics,
	[property: JsonPropertyName("skills")] IReadOnlyList<Skill> Skills,
	[property: JsonPropertyName("manager")] Manager Manager,
	[property: JsonPropertyName("salary")] Salary Salary,
	[property: JsonPropertyName("address")] Address Address,
	[property: JsonPropertyName("createdAt")] string CreatedAt,
	[property: JsonPropertyName("publishedAt")] string PublishedAt,
	[property: JsonPropertyName("expiresAt")] string ExpiresAt,
	[property: JsonPropertyName("rotationAccess")] bool? RotationAccess,
	[property: JsonPropertyName("slug")] string Slug,
	[property: JsonPropertyName("country")] Country Country
);

public record Salary(

);

public record Metro(

);

public record Skill(
	[property: JsonPropertyName("name")] string Name
);

