using BaseFaq.AI.Business.Common.Extensions;
using BaseFaq.AI.Business.Generation.Abstractions;
using BaseFaq.AI.Business.Generation.Commands.ProcessFaqGenerationRequested;
using BaseFaq.AI.Business.Generation.Consumers;
using BaseFaq.AI.Business.Generation.Service;
using BaseFaq.Common.Infrastructure.MassTransit.Extensions;
using BaseFaq.Common.Infrastructure.MassTransit.Models;
using BaseFaq.Models.Ai.Contracts.Generation;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace BaseFaq.AI.Business.Generation.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAiGenerationBusiness(this IServiceCollection services)
    {
        services.AddAiBusinessCommon();
        services.AddScoped<IFaqGenerationEngine, SimpleFaqGenerationEngine>();
        services.AddMediatR(config =>
            config.RegisterServicesFromAssemblyContaining<ProcessFaqGenerationRequestedCommandHandler>());

        return services;
    }

    public static IBusRegistrationConfigurator AddGenerationWorker(this IBusRegistrationConfigurator bus)
    {
        bus.AddConsumer<FaqGenerationRequestedConsumer>();
        return bus;
    }

    public static void ConfigureGenerationWorker(
        this IRabbitMqBusFactoryConfigurator cfg,
        IBusRegistrationContext context,
        RabbitMqOption rabbitMqOption)
    {
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
    }
}