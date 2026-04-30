using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Common.Infrastructure.Core.Constants;
using BaseFaq.Models.Common.Enums;
using Microsoft.AspNetCore.Http;

namespace BaseFaq.Common.Infrastructure.Core.Services;

public sealed class SessionService : ISessionService
{
    private readonly IClaimService _claimService;
    private readonly IUserIdProvider _userIdProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SessionService(
        IClaimService claimService,
        IUserIdProvider userIdProvider,
        IHttpContextAccessor httpContextAccessor)
    {
        _claimService = claimService;
        _userIdProvider = userIdProvider;
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid GetTenantId(ModuleEnum module)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null ||
            !httpContext.Items.TryGetValue(TenantContextKeys.TenantIdItemKey, out var tenantIdValue) ||
            tenantIdValue is not Guid tenantId)
        {
            throw new ApiErrorException(
                "Tenant context is missing from the current request.",
                errorCode: (int)HttpStatusCode.BadRequest);
        }

        return tenantId;
    }

    public Guid GetUserId()
    {
        var userId = _userIdProvider.GetUserId();

        return userId;
    }

    public string? GetUserName()
    {
        return _claimService.GetName() ?? _claimService.GetEmail();
    }
}
