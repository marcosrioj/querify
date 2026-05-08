using MassTransit;
using Querify.Common.Infrastructure.MassTransit.Extensions;
using Querify.Common.Infrastructure.MassTransit.Models;
using Querify.Models.QnA.Events;
using Querify.QnA.Worker.Api.Consumers;
using Querify.QnA.Worker.Api.Events;
using Querify.QnA.Worker.Business.Source.Abstractions;
using Querify.QnA.Worker.Business.Source.Extensions;
using RabbitMQ.Client;

namespace Querify.QnA.Worker.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddQnAWorkerFeatures(this IServiceCollection services, IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddSourceWorker(configuration, environment);
        services.AddSourceUploadMessaging(configuration);
    }

    private static void AddSourceUploadMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ISourceUploadStatusChangedEventPublisher, MassTransitSourceUploadStatusChangedEventPublisher>();

        services.AddMassTransit(config =>
        {
            config.AddConsumer<SourceUploadCompletedConsumer>();

            config.UsingRabbitMq((context, cfg) =>
            {
                var rabbitMqOption = configuration.GetRequiredSection(RabbitMqOption.Name).Get<RabbitMqOption>() ??
                    throw new InvalidOperationException("RabbitMQ options are not configured.");

                cfg.Host(rabbitMqOption.Hostname, (ushort)rabbitMqOption.Port, "/", host =>
                {
                    host.Username(rabbitMqOption.Username);
                    host.Password(rabbitMqOption.Password);
                });

                cfg.Message<SourceUploadCompletedIntegrationEvent>(message =>
                    message.SetEntityName(SourceUploadIntegrationEventNames.CompletedExchangeName));
                cfg.Publish<SourceUploadCompletedIntegrationEvent>(publish =>
                    publish.ExchangeType = ExchangeType.Fanout);

                cfg.Message<SourceUploadStatusChangedIntegrationEvent>(message =>
                    message.SetEntityName(SourceUploadIntegrationEventNames.StatusChangedExchangeName));
                cfg.Publish<SourceUploadStatusChangedIntegrationEvent>(publish =>
                    publish.ExchangeType = ExchangeType.Fanout);

                cfg.ReceiveEndpoint(SourceUploadIntegrationEventNames.CompletedQueueName, endpoint =>
                {
                    endpoint.ConfigureConsumeTopology = false;
                    endpoint.PrefetchCount = (ushort)Math.Max(1, rabbitMqOption.PrefetchCount);
                    endpoint.ConcurrentMessageLimit = Math.Max(1, rabbitMqOption.ConcurrencyLimit);
                    endpoint.ConfigureResilience(rabbitMqOption);
                    endpoint.Bind(SourceUploadIntegrationEventNames.CompletedExchangeName, bind =>
                    {
                        bind.ExchangeType = ExchangeType.Fanout;
                    });
                    endpoint.ConfigureConsumer<SourceUploadCompletedConsumer>(context);
                });
            });
        });
    }
}
