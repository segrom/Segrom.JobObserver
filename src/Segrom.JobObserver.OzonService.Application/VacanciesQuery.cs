namespace Segrom.JobObserver.OzonService.Application;

public record VacanciesQuery(
	int Limit,
	int Offset,
	long[]? Ids = null,
	string[]? Cities = null,
	bool? IsClosed = null
);