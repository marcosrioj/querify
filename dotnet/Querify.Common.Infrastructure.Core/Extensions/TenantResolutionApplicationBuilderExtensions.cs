using Querify.Common.Infrastructure.Core.Middleware;
using Querify.Models.Common.Enums;
using Microsoft.AspNetCore.Builder;

namespace Querify.Common.Infrastructure.Core.Extensions;

public static class TenantResolutionApplicationBuilderExtensions
{
    public static IApplicationBuilder UseTenantResolution(this IApplicationBuilder app, ModuleEnum module)
    {
        return app.UseWhen(
            context =>
                !context.Request.Path.StartsWithSegments("/swagger", StringComparison.OrdinalIgnoreCase) &&
                !context.Request.Path.StartsWithSegments("/openapi", StringComparison.OrdinalIgnoreCase),
            branch => branch.UseMiddleware<TenantResolutionMiddleware>(new TenantResolutionOptions
            {
                Module = module
            }));
    }
}
