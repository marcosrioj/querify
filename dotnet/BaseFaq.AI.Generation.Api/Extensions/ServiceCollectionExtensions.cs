using BaseFaq.AI.Generation.Business.Worker.Extensions;
using BaseFaq.AI.Common.Persistence.AiDb.Extensions;
using BaseFaq.Common.EntityFramework.Tenant.Extensions;

namespace BaseFaq.AI.Generation.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddFeatures(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTenantDb(configuration.GetConnectionString("TenantDb"));
        services.AddAiDb(configuration.GetConnectionString("AiDb"));
        services.AddGenerationWorker(configuration);
    }
}