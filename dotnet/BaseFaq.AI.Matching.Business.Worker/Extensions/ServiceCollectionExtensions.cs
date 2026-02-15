using BaseFaq.AI.Common.Contracts.Matching;
using BaseFaq.AI.Common.Providers.Abstractions;
using BaseFaq.AI.Common.Providers.Options;
using BaseFaq.AI.Common.Providers.Service;
using BaseFaq.AI.Matching.Business.Worker.Abstractions;
using BaseFaq.AI.Matching.Business.Worker.Commands.ProcessFaqMatchingRequested;
using BaseFaq.AI.Matching.Business.Worker.Consumers;
using BaseFaq.AI.Matching.Business.Worker.Service;
using BaseFaq.Common.Infrastructure.MassTransit.Extensions;
using BaseFaq.Common.Infrastructure.MassTransit.Models;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BaseFaq.AI.Matching.Business.Worker.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMatchingWorker(this IServiceCollection services, IConfiguration configuration)
    {
        var rabbitMqOption = configuration.GetSection(RabbitMqOption.Name).Get<RabbitMqOption>()
                             ?? throw new InvalidOperationException("RabbitMQ configuration is missing.");

        services.AddOptions<AiProviderOptions>()
            .Bind(configuration.GetSection(AiProviderOptions.Name))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<AiProviderOptions>, AiProviderOptionsValidator>();

        services.AddScoped<IAiProviderCredentialAccessor, AiProviderCredentialAccessor>();
        services.AddScoped<IMatchingFaqDbContextFactory, MatchingFaqDbContextFactory>();
        services.AddScoped<IMatchingPromptComposer, MatchingPromptComposer>();
        services.AddMediatR(config =>
            config.RegisterServicesFromAssemblyContaining<ProcessFaqMatchingRequestedCommandHandler>());

        services.AddMassTransit(x =>
        {
            x.AddConsumer<FaqMatchingRequestedConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(new Uri($"rabbitmq://{rabbitMqOption.Hostname}:{rabbitMqOption.Port}/"), h =>
                {
                    h.Username(rabbitMqOption.Username);
                    h.Password(rabbitMqOption.Password);
                });

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
            });
        });

        return services;
    }
}