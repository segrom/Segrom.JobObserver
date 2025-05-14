using System.Text.Json.Serialization;

namespace Segrom.JobObserver.OzonService.Domain;

public record VacancyInfo (
	[property: JsonPropertyName("full_description")] 
	string FullDescription,
	[property: JsonPropertyName("skills")] 
	IReadOnlyList<string> Skills,
	[property: JsonPropertyName("slug")] 
	string Slug,
	[property: JsonPropertyName("url")] 
	string Url);