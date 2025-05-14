using Prometheus;
using Segrom.JobObserver.TelegramBot.Application.Extensions;
using Segrom.JobObserver.TelegramBot.Client.Extensions;
using Segrom.JobObserver.TelegramBot.Host.Extensions;
using Segrom.JobObserver.TelegramBot.KafkaConsumer.Extensions;
using Segrom.JobObserver.TelegramBot.KafkaConsumer.Options;
using Segrom.JobObserver.TelegramBot.OzonServiceGrpcClient.Extensions;
using Segrom.JobObserver.TelegramBot.PostgresRepository.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<KafkaConsumerOptions>()
	.BindConfiguration(nameof(KafkaConsumerOptions));

builder.Services.AddHealthChecks()
	.ForwardToPrometheus();

// Services
builder.Services.AddKafkaConsumers();
builder.Services.AddBotClient();
builder.Services.AddMessagesRepository();
builder.Services.AddOzonVacancyApiClient();
builder.Services.AddApplication();

// Logging
builder.AddGraylog();

var app = builder.Build();

app.MapMetrics();
app.MapGet("/", () => "Bot is working!");
app.MapHealthChecks("/health");

app.RunMessagesRepositoryMigrations();
app.RunBotClient();

app.Run();