using BaseFaq.AI.Business.Generation.Commands.ProcessFaqGenerationRequested;
using BaseFaq.AI.Business.Generation.Consumers;
using BaseFaq.AI.Business.Matching.Commands.ProcessFaqMatchingRequested;
using BaseFaq.AI.Business.Matching.Consumers;
using BaseFaq.AI.Common.Contracts.Generation;
using BaseFaq.AI.Common.Contracts.Matching;
using BaseFaq.AI.Common.Persistence.AiDb.Extensions;
using BaseFaq.Common.EntityFramework.Tenant.Extensions;
using BaseFaq.Common.Infrastructure.MassTransit.Extensions;
using BaseFaq.Common.Infrastructure.MassTransit.Models;
using BaseFaq.Faq.Common.Persistence.FaqDb.Extensions;
using MassTransit;

namespace BaseFaq.AI.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddFeatures(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTenantDb(configuration.GetConnectionString("TenantDb"));
        services.AddFaqDb();
        services.AddAiDb(configuration.GetConnectionString("AiDb"));

        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssemblyContaining<ProcessFaqGenerationRequestedCommandHandler>();
            config.RegisterServicesFromAssemblyContaining<ProcessFaqMatchingRequestedCommandHandler>();
        });

        var generationRabbitMq = GetRabbitMqOption(configuration, "RabbitMQ:Generation", "generation");
        var matchingRabbitMq = GetRabbitMqOption(configuration, "RabbitMQ:Matching", "matching");

        EnsureRabbitMqHostConsistency(generationRabbitMq, matchingRabbitMq);

        services.AddMassTransit(x =>
        {
            x.AddConsumer<FaqGenerationRequestedConsumer>();
            x.AddConsumer<FaqMatchingRequestedConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(new Uri($"rabbitmq://{generationRabbitMq.Hostname}:{generationRabbitMq.Port}/"), h =>
                {
                    h.Username(generationRabbitMq.Username);
                    h.Password(generationRabbitMq.Password);
                });

                ConfigureGenerationWorker(cfg, context, generationRabbitMq);
                ConfigureMatchingWorker(cfg, context, matchingRabbitMq);
            });
        });
    }

    private static RabbitMqOption GetRabbitMqOption(
        IConfiguration configuration,
        string sectionName,
        string workerName)
    {
        return configuration.GetSection(sectionName).Get<RabbitMqOption>()
               ?? throw new InvalidOperationException(
                   $"RabbitMQ configuration is missing for {workerName} worker at '{sectionName}'.");
    }

    private static void EnsureRabbitMqHostConsistency(RabbitMqOption generation, RabbitMqOption matching)
    {
        if (!string.Equals(generation.Hostname, matching.Hostname, StringComparison.OrdinalIgnoreCase) ||
            generation.Port != matching.Port ||
            !string.Equals(generation.Username, matching.Username, StringComparison.Ordinal) ||
            !string.Equals(generation.Password, matching.Password, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Generation and matching RabbitMQ host credentials must match when hosted in the same API process.");
        }
    }

    private static void ConfigureGenerationWorker(
        IRabbitMqBusFactoryConfigurator cfg,
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

    private static void ConfigureMatchingWorker(
        IRabbitMqBusFactoryConfigurator cfg,
        IBusRegistrationContext context,
        RabbitMqOption rabbitMqOption)
    {
        cfg.Message<FaqMatchingRequestedV1>(message =>
            message.SetEntityName(rabbitMqOption.Exchange.Name));

        cfg.Publish<FaqMatchingRequestedV1>(message =>
            message.ExchangeType = rabbitMqOption.Exchange.Type);

        cfg.Message<FaqMatchingCompletedV1>(message =>
            message.SetEntityName(MatchingEventNames.CompletedExchange));

        cfg.Publish<FaqMatchingCompletedV1>(message =>
            message.ExchangeType = MatchingEventNames.ExchangeType);

        cfg.Message<FaqMatchingFailedV1>(message =>
            message.SetEntityName(MatchingEventNames.FailedExchange));

        cfg.Publish<FaqMatchingFailedV1>(message =>
            message.ExchangeType = MatchingEventNames.ExchangeType);

        cfg.ReceiveEndpoint(rabbitMqOption.QueueName, endpoint =>
        {
            endpoint.PrefetchCount = (ushort)Math.Max(1, rabbitMqOption.PrefetchCount);
            endpoint.ConcurrentMessageLimit = Math.Max(1, rabbitMqOption.ConcurrencyLimit);
            endpoint.ConfigureResilience(rabbitMqOption);
            endpoint.ConfigureConsumer<FaqMatchingRequestedConsumer>(context);
        });
    }
}