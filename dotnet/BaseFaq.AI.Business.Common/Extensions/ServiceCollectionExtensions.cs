using BaseFaq.AI.Business.Common.Abstractions;
using BaseFaq.AI.Business.Common.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace BaseFaq.AI.Business.Common.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAiBusinessCore(this IServiceCollection services)
    {
        services.AddScoped<IFaqDbContextFactory, FaqDbContextFactory>();
        return services;
    }
}