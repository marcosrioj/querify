using BaseFaq.AI.Business.Generation.Commands.ProcessFaqGenerationRequested;
using BaseFaq.AI.Business.Generation.Consumers;
using BaseFaq.Common.Infrastructure.MassTransit.Extensions;
using BaseFaq.Common.Infrastructure.MassTransit.Models;
using BaseFaq.Models.Ai.Contracts.Generation;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BaseFaq.AI.Business.Generation.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGenerationWorker(
        this IServiceCollection services,
        IConfiguration configuration,
        string rabbitMqSectionName = RabbitMqOption.Name)
    {
        var rabbitMqOption = configuration.GetSection(rabbitMqSectionName).Get<RabbitMqOption>()
                             ?? throw new InvalidOperationException(
                                 $"RabbitMQ configuration is missing at '{rabbitMqSectionName}'.");

        services.AddMediatR(config =>
            config.RegisterServicesFromAssemblyContaining<ProcessFaqGenerationRequestedCommandHandler>());

        services.AddMassTransit(x =>
        {
            x.AddConsumer<FaqGenerationRequestedConsumer>();

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

                cfg.ReceiveEndpoint(rabbitMqOption.QueueName, endpoint =>
                {
                    endpoint.PrefetchCount = (ushort)Math.Max(1, rabbitMqOption.PrefetchCount);
                    endpoint.ConcurrentMessageLimit = Math.Max(1, rabbitMqOption.ConcurrencyLimit);
                    endpoint.ConfigureResilience(rabbitMqOption);
                    endpoint.ConfigureConsumer<FaqGenerationRequestedConsumer>(context);
                });
            });
        });

        return services;
    }
}