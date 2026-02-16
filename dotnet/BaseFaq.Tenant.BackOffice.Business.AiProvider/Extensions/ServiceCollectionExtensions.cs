using BaseFaq.Tenant.BackOffice.Business.AiProvider.Abstractions;
using BaseFaq.Tenant.BackOffice.Business.AiProvider.Commands.CreateAiProvider;
using BaseFaq.Tenant.BackOffice.Business.AiProvider.Service;
using Microsoft.Extensions.DependencyInjection;

namespace BaseFaq.Tenant.BackOffice.Business.AiProvider.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAiProviderBusiness(this IServiceCollection services)
    {
        services.AddScoped<IAiProviderService, AiProviderService>();
        services.AddMediatR(config =>
            config.RegisterServicesFromAssemblyContaining<AiProvidersCreateAiProviderCommandHandler>());

        return services;
    }
}