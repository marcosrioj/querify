using BaseFaq.AI.Business.Generation.Extensions;
using BaseFaq.AI.Business.Matching.Extensions;
using BaseFaq.Common.EntityFramework.Tenant.Extensions;
using BaseFaq.Common.Infrastructure.MassTransit.Models;
using MassTransit;

namespace BaseFaq.AI.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddFeatures(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddTenantDb(configuration.GetConnectionString("TenantDb"));
        services.AddAiGenerationBusiness();
        services.AddAiMatchingBusiness();

        var generationRabbitMq = GetRabbitMqOption(configuration, "RabbitMQ:Generation", "generation");
        var matchingRabbitMq = GetRabbitMqOption(configuration, "RabbitMQ:Matching", "matching");

        services.AddMassTransit(x =>
        {
            x.AddGenerationWorker();
            x.AddMatchingWorker();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(new Uri($"rabbitmq://{generationRabbitMq.Hostname}:{generationRabbitMq.Port}/"), h =>
                {
                    h.Username(generationRabbitMq.Username);
                    h.Password(generationRabbitMq.Password);
                });

                cfg.ConfigureGenerationWorker(context, generationRabbitMq);
                cfg.ConfigureMatchingWorker(context, matchingRabbitMq);
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
}
