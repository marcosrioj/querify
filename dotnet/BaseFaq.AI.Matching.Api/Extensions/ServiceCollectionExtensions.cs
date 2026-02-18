using BaseFaq.AI.Common.Persistence.AiDb.Extensions;
using BaseFaq.AI.Matching.Business.Worker.Extensions;
using BaseFaq.Common.EntityFramework.Tenant.Extensions;
using BaseFaq.Faq.Common.Persistence.FaqDb.Extensions;

namespace BaseFaq.AI.Matching.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddFeatures(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTenantDb(configuration.GetConnectionString("TenantDb"));
        services.AddFaqDb();
        services.AddAiDb(configuration.GetConnectionString("AiDb"));
        services.AddMatchingWorker(configuration);
    }
}