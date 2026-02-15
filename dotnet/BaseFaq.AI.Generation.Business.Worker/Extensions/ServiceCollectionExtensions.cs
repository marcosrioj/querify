using BaseFaq.AI.Common.Contracts.Generation;
using BaseFaq.AI.Common.Providers.Abstractions;
using BaseFaq.AI.Common.Providers.Options;
using BaseFaq.AI.Common.Providers.Service;
using BaseFaq.AI.Generation.Business.Worker.Commands.ProcessFaqGenerationRequested;
using BaseFaq.AI.Generation.Business.Worker.Consumers;
using BaseFaq.AI.Generation.Business.Worker.Abstractions;
using BaseFaq.AI.Generation.Business.Worker.Service;
using BaseFaq.Common.Infrastructure.MassTransit.Extensions;
using BaseFaq.Common.Infrastructure.MassTransit.Models;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BaseFaq.AI.Generation.Business.Worker.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGenerationWorker(this IServiceCollection services, IConfiguration configuration)
    {
        var rabbitMqOption = configuration.GetSection(RabbitMqOption.Name).Get<RabbitMqOption>()
                             ?? throw new InvalidOperationException("RabbitMQ configuration is missing.");

        services.AddOptions<AiProviderOptions>()
            .Bind(configuration.GetSection(AiProviderOptions.Name))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<AiProviderOptions>, AiProviderOptionsValidator>();

        services.AddScoped<IAiProviderCredentialAccessor, AiProviderCredentialAccessor>();
        services.AddScoped<IFaqIntegrationDbContextFactory, FaqIntegrationDbContextFactory>();
        services.AddScoped<IGenerationFaqWriteService, GenerationFaqWriteService>();
        services.AddScoped<IGenerationPromptComposer, GenerationPromptComposer>();
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