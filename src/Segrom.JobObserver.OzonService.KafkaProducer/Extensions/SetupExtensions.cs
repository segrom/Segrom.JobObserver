using Microsoft.Extensions.DependencyInjection;
using Segrom.JobObserver.OzonService.KafkaProducer.Abstractions;

namespace Segrom.JobObserver.OzonService.KafkaProducer.Extensions;

public static class SetupExtensions
{
    public static IServiceCollection AddKafkaProducerWithOutbox(this IServiceCollection services)
    {
        services.AddSingleton<IKafkaOutboxProducer, KafkaOutboxProducer>();
        services.AddHostedService<OutboxWorker>();
        return services;
    }
}