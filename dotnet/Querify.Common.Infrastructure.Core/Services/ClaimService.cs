using System;
using System.Security.Claims;
using Querify.Common.Infrastructure.Core.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Querify.Common.Infrastructure.Core.Services;

public class ClaimService(IHttpContextAccessor httpContextAccessor) : IClaimService
{
    private const string ExternalUserIdClaimType = "sub";

    public string? GetName()
    {
        var user = httpContextAccessor.HttpContext?.User;
        return FindClaimBySuffix(user, "/name") ??
               user?.FindFirstValue("name") ??
               user?.FindFirstValue(ClaimTypes.Name);
    }

    public string? GetEmail()
    {
        var user = httpContextAccessor.HttpContext?.User;
        return FindClaimBySuffix(user, "/email") ??
               user?.FindFirstValue("email") ??
               user?.FindFirstValue(ClaimTypes.Email);
    }

    public string? GetExternalUserId()
    {
        var user = httpContextAccessor.HttpContext?.User;
        return user?.FindFirstValue(ExternalUserIdClaimType);
    }

    private static string? FindClaimBySuffix(ClaimsPrincipal? user, string suffix)
    {
        return user?.Claims.FirstOrDefault(claim => claim.Type.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            ?.Value;
    }
}