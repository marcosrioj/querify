using BaseFaq.AI.Business.Common.Extensions;
using BaseFaq.AI.Business.Matching.Abstractions;
using BaseFaq.AI.Business.Matching.Commands.ProcessFaqMatchingRequested;
using BaseFaq.AI.Business.Matching.Consumers;
using BaseFaq.AI.Business.Matching.Service;
using BaseFaq.Common.Infrastructure.MassTransit.Extensions;
using BaseFaq.Common.Infrastructure.MassTransit.Models;
using BaseFaq.Models.Ai.Contracts.Matching;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace BaseFaq.AI.Business.Matching.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAiMatchingBusiness(this IServiceCollection services)
    {
        services.AddAiBusinessCommon();
        services.AddScoped<IMatchingExecutionService, MatchingExecutionService>();
        services.AddScoped<IMatchingProviderClient, MatchingProviderClient>();

        services.AddMediatR(config =>
            config.RegisterServicesFromAssemblyContaining<ProcessFaqMatchingRequestedCommandHandler>());

        return services;
    }

    public static IBusRegistrationConfigurator AddMatchingWorker(this IBusRegistrationConfigurator bus)
    {
        bus.AddConsumer<FaqMatchingRequestedConsumer>();
        return bus;
    }

    public static void ConfigureMatchingWorker(
        this IRabbitMqBusFactoryConfigurator cfg,
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