using MassTransit;
using Querify.Common.Infrastructure.MassTransit.Extensions;
using Querify.Common.Infrastructure.MassTransit.Models;
using Querify.Common.Infrastructure.Signalr.Portal.Extensions;
using Querify.Models.Common.Enums;
using Querify.Models.QnA.Events;
using Querify.QnA.Portal.Business.Source.Events;
using RabbitMQ.Client;

namespace Querify.QnA.Portal.Api.Extensions;

public static class EventsServiceCollectionExtensions
{
    public static IServiceCollection AddEventsFeature(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPortalSignalR(configuration, ModuleEnum.QnA);

        services.AddMassTransit(config =>
        {
            config.AddConsumer<SourceUploadStatusChangedConsumer>();

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

                cfg.ReceiveEndpoint(SourceUploadIntegrationEventNames.StatusChangedQueueName, endpoint =>
                {
                    endpoint.ConfigureConsumeTopology = false;
                    endpoint.PrefetchCount = (ushort)Math.Max(1, rabbitMqOption.PrefetchCount);
                    endpoint.ConcurrentMessageLimit = Math.Max(1, rabbitMqOption.ConcurrencyLimit);
                    endpoint.ConfigureResilience(rabbitMqOption);
                    endpoint.Bind(SourceUploadIntegrationEventNames.StatusChangedExchangeName, bind =>
                    {
                        bind.ExchangeType = ExchangeType.Fanout;
                    });
                    endpoint.ConfigureConsumer<SourceUploadStatusChangedConsumer>(context);
                });
            });
        });

        return services;
    }
}
