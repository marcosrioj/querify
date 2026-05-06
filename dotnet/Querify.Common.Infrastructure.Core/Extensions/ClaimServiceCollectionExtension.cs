using Querify.Common.Infrastructure.Core.Abstractions;
using Querify.Common.Infrastructure.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Querify.Common.Infrastructure.Core.Extensions;

public static class CLaimServiceCollectionExtension
{
    public static IServiceCollection AddClaimService(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpContextAccessor();

        services.AddTransient<IClaimService, ClaimService>();

        return services;
    }
}