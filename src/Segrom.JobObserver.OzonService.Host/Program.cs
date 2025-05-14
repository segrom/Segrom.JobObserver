using Prometheus;
using Segrom.JobObserver.OzonService.Application.Extensions;
using Segrom.JobObserver.OzonService.Application.Options;
using Segrom.JobObserver.OzonService.Host;
using Segrom.JobObserver.OzonService.Host.Extensions;
using Segrom.JobObserver.OzonService.Host.Grpc;
using Segrom.JobObserver.OzonService.KafkaProducer.Extensions;
using Segrom.JobObserver.OzonService.OzonJobApiClient.Extensions;
using Segrom.JobObserver.OzonService.PostgresRepository.Extensions;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<MessageBrokerOptions>()
	.BindConfiguration(nameof(MessageBrokerOptions));
builder.Services.AddOptions<OutboxOptions>()
	.BindConfiguration(nameof(OutboxOptions));

builder.Services.AddHealthChecks()
	.ForwardToPrometheus();

// Services
builder.Services.AddPostgresRepository();
builder.Services.AddOzonJobApiClient();
builder.Services.AddKafkaProducerWithOutbox();
builder.Services.AddApplication();
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();
builder.Services.AddApplication();
builder.Services.AddHostedService<VacancyUpdateWorker>();

// Logging
builder.AddGraylog();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<GrpcOzonService>();
app.MapGrpcReflectionService();

app.MapMetrics();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");
app.MapHealthChecks("/health");

app.RunPostgresRepositoryMigrations();
app.Run();