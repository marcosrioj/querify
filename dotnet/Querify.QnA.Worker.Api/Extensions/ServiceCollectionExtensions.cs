using MassTransit;
using Querify.Common.Infrastructure.MassTransit.Extensions;
using Querify.Common.Infrastructure.MassTransit.Models;
using Querify.Models.QnA.Dtos.IntegrationEvents;
using Querify.QnA.Worker.Business.Source.Consumers;
using Querify.QnA.Worker.Business.Source.Extensions;

namespace Querify.QnA.Worker.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddQnAWorkerFeatures(this IServiceCollection services, IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddSourceWorker(configuration, environment);
        services.AddQnAMassTransit(configuration);
    }

    private static void AddQnAMassTransit(this IServiceCollection services, IConfiguration configuration)
    {
        var rabbitMqOption = configuration.GetSection(RabbitMqOption.Name).Get<RabbitMqOption>()
                             ?? throw new InvalidOperationException("RabbitMQ options were not found.");

        services.AddMassTransit(config =>
        {
            config.AddConsumer<SourceUploadedConsumer>();

            config.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(rabbitMqOption.Hostname, (ushort)rabbitMqOption.Port, "/", host =>
                {
                    host.Username(rabbitMqOption.Username);
                    host.Password(rabbitMqOption.Password);
                });

                cfg.Message<SourceUploadedIntegrationEvent>(message =>
                    message.SetEntityName("qna.source.uploaded.v1"));
                cfg.Publish<SourceUploadedIntegrationEvent>(publish =>
                    publish.ExchangeType = rabbitMqOption.Exchange.Type);

                cfg.ReceiveEndpoint("qna.source.uploaded", endpoint =>
                {
                    endpoint.PrefetchCount = (ushort)Math.Max(1, rabbitMqOption.PrefetchCount);
                    endpoint.ConcurrentMessageLimit = Math.Max(1, rabbitMqOption.ConcurrencyLimit);
                    endpoint.ConfigureResilience(rabbitMqOption);
                    endpoint.ConfigureConsumer<SourceUploadedConsumer>(context);
                });
            });
        });
    }
}
