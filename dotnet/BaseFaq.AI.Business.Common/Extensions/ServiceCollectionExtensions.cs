using BaseFaq.AI.Business.Common.Abstractions;
using BaseFaq.AI.Business.Common.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BaseFaq.AI.Business.Common.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAiBusinessCommon(this IServiceCollection services)
    {
        services.TryAddScoped<IFaqDbContextFactory, FaqDbContextFactory>();
        return services;
    }
}