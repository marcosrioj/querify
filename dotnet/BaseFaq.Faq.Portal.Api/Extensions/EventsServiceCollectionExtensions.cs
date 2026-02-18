using BaseFaq.Common.Infrastructure.MassTransit.Extensions;
using BaseFaq.Common.Infrastructure.MassTransit.Models;
using BaseFaq.Faq.Portal.Api.Consumers;
using BaseFaq.Models.Ai.Contracts.Generation;
using MassTransit;

namespace BaseFaq.Faq.Portal.Api.Extensions;

public static class EventsServiceCollectionExtensions
{
    public static IServiceCollection AddEventsFeature(this IServiceCollection services, IConfiguration configuration)
    {
        var rabbitMqOption = configuration.GetSection(RabbitMqOption.Name).Get<RabbitMqOption>()
                             ?? throw new InvalidOperationException("RabbitMQ configuration is missing.");

        services.AddMassTransit(x =>
        {
            x.AddConsumer<FaqGenerationReadyConsumer>();
            x.AddConsumer<FaqGenerationFailedConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(new Uri($"rabbitmq://{rabbitMqOption.Hostname}:{rabbitMqOption.Port}/"), h =>
                {
                    h.Username(rabbitMqOption.Username);
                    h.Password(rabbitMqOption.Password);
                });

                cfg.Message<FaqGenerationRequestedV1>(message =>
                    message.SetEntityName(rabbitMqOption.Exchange.Name));

                cfg.Publish<FaqGenerationRequestedV1>(message =>
                    message.ExchangeType = rabbitMqOption.Exchange.Type);

                cfg.Message<FaqGenerationReadyV1>(message =>
                    message.SetEntityName(GenerationEventNames.ReadyExchange));

                cfg.Publish<FaqGenerationReadyV1>(message =>
                    message.ExchangeType = GenerationEventNames.ExchangeType);

                cfg.Message<FaqGenerationFailedV1>(message =>
                    message.SetEntityName(GenerationEventNames.FailedExchange));

                cfg.Publish<FaqGenerationFailedV1>(message =>
                    message.ExchangeType = GenerationEventNames.ExchangeType);

                cfg.ReceiveEndpoint(GenerationEventNames.ReadyCallbackQueue, endpoint =>
                {
                    endpoint.PrefetchCount = (ushort)Math.Max(1, rabbitMqOption.PrefetchCount);
                    endpoint.ConcurrentMessageLimit = Math.Max(1, rabbitMqOption.ConcurrencyLimit);
                    endpoint.ConfigureResilience(rabbitMqOption);
                    endpoint.ConfigureConsumer<FaqGenerationReadyConsumer>(context);
                });

                cfg.ReceiveEndpoint(GenerationEventNames.FailedCallbackQueue, endpoint =>
                {
                    endpoint.PrefetchCount = (ushort)Math.Max(1, rabbitMqOption.PrefetchCount);
                    endpoint.ConcurrentMessageLimit = Math.Max(1, rabbitMqOption.ConcurrencyLimit);
                    endpoint.ConfigureResilience(rabbitMqOption);
                    endpoint.ConfigureConsumer<FaqGenerationFailedConsumer>(context);
                });
            });
        });

        return services;
    }
}