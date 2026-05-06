using System.Net;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Common.Infrastructure.Core.Abstractions;
using Querify.Common.Infrastructure.Core.Attributes;
using Querify.Common.Infrastructure.Core.Constants;
using Querify.Models.Common.Enums;
using Microsoft.AspNetCore.Http;

namespace Querify.Common.Infrastructure.Core.Middleware;

public sealed class TenantResolutionMiddleware(
    RequestDelegate next,
    TenantResolutionOptions options)
{
    public const string TenantHeaderName = "X-Tenant-Id";

    public async Task Invoke(
        HttpContext context,
        ISessionService sessionService,
        IAllowedTenantStore allowedTenantStore,
        IAllowedTenantProvider allowedTenantProvider)
    {
        var skipTenantValidation = IsTenantAccessValidationSkipped(context);

        if (!context.Request.Headers.TryGetValue(TenantHeaderName, out var headerValues))
        {
            if (skipTenantValidation)
            {
                await next(context);
                return;
            }

            throw new ApiErrorException(
                $"Missing required header '{TenantHeaderName}'.",
                errorCode: (int)HttpStatusCode.BadRequest);
        }

        var rawTenantId = headerValues.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(rawTenantId) || !Guid.TryParse(rawTenantId, out var tenantId))
        {
            throw new ApiErrorException(
                $"Header '{TenantHeaderName}' must be a valid GUID.",
                errorCode: (int)HttpStatusCode.BadRequest);
        }

        if (!skipTenantValidation)
        {
            var userId = sessionService.GetUserId();
            var allowedTenants = await allowedTenantStore.GetAllowedTenantIds(userId, context.RequestAborted);
            if (allowedTenants is null)
            {
                allowedTenants = await allowedTenantProvider.GetAllowedTenantIds(userId, context.RequestAborted);
                await allowedTenantStore.SetAllowedTenantIds(userId, allowedTenants,
                    cancellationToken: context.RequestAborted);
            }

            if (!IsTenantAllowed(allowedTenants, options.Module, tenantId))
            {
                throw new ApiErrorException(
                    $"Tenant '{tenantId}' is not allowed for the current user.",
                    errorCode: (int)HttpStatusCode.Forbidden);
            }
        }

        context.Items[TenantContextKeys.TenantIdItemKey] = tenantId;

        await next(context);
    }

    private static bool IsTenantAccessValidationSkipped(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        return endpoint?.Metadata.GetMetadata<SkipTenantAccessValidationAttribute>() is not null;
    }

    private static bool IsTenantAllowed(IReadOnlyDictionary<string, IReadOnlyCollection<Guid>> allowedTenants,
        ModuleEnum module,
        Guid tenantId)
    {
        var moduleKey = module.ToString();
        return allowedTenants.TryGetValue(moduleKey, out var tenantIds) && tenantIds.Contains(tenantId);
    }
}

public sealed class TenantResolutionOptions
{
    public ModuleEnum Module { get; init; }
}
