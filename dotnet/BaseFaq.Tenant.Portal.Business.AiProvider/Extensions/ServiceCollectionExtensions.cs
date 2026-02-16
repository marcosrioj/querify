using BaseFaq.Tenant.Portal.Business.AiProvider.Abstractions;
using BaseFaq.Tenant.Portal.Business.AiProvider.Queries.GetAiProviderList;
using BaseFaq.Tenant.Portal.Business.AiProvider.Service;
using Microsoft.Extensions.DependencyInjection;

namespace BaseFaq.Tenant.Portal.Business.AiProvider.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAiProviderBusiness(this IServiceCollection services)
    {
        services.AddScoped<IAiProviderService, AiProviderService>();
        services.AddMediatR(config =>
            config.RegisterServicesFromAssemblyContaining<AiProvidersGetAiProviderListQueryHandler>());

        return services;
    }
}