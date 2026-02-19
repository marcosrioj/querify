using BaseFaq.AI.Business.Generation.Extensions;
using BaseFaq.AI.Business.Matching.Extensions;
using BaseFaq.Common.EntityFramework.Tenant.Extensions;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Common.Infrastructure.MassTransit.Models;
using BaseFaq.Faq.Common.Persistence.FaqDb.Extensions;
using BaseFaq.Models.Common.Enums;
using MassTransit;

namespace BaseFaq.AI.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddFeatures(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ISessionService, AiWorkerSessionService>();
        services.AddTenantDb(configuration.GetConnectionString("TenantDb"));
        services.AddFaqDb();
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

    private sealed class AiWorkerSessionService : ISessionService
    {
        public Guid GetTenantId(AppEnum app)
        {
            throw new InvalidOperationException(
                "AI worker session tenant is not available. Use tenant-aware message payloads with IFaqDbContextFactory.");
        }

        public Guid GetUserId() => Guid.Empty;
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