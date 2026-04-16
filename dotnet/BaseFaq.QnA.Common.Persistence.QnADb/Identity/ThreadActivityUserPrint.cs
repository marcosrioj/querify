using System.Security.Cryptography;
using System.Text;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using Microsoft.AspNetCore.Http;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Identity;

public static class ThreadActivityUserPrint
{
    public static ThreadActivityUserIdentity ResolveCurrent(
        HttpContext httpContext,
        IClaimService claimService,
        ISessionService sessionService)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(claimService);
        ArgumentNullException.ThrowIfNull(sessionService);

        var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        var ip = string.IsNullOrWhiteSpace(forwardedFor)
            ? httpContext.Connection.RemoteIpAddress?.ToString()
            : forwardedFor.Split(',')[0].Trim();
        ip ??= string.Empty;

        var userAgent = httpContext.Request.Headers.UserAgent.ToString();
        var authenticatedUserId = ResolveAuthenticatedUserId(claimService, sessionService);
        var userPrint = authenticatedUserId?.ToString("D") ?? ComputeAnonymousUserPrint(ip, userAgent);

        return new ThreadActivityUserIdentity(userPrint, ip, userAgent, authenticatedUserId);
    }

    public static string? ResolveStored(string? explicitUserPrint, string? metadataUserPrint)
    {
        if (!string.IsNullOrWhiteSpace(explicitUserPrint))
            return explicitUserPrint;

        return string.IsNullOrWhiteSpace(metadataUserPrint) ? null : metadataUserPrint;
    }

    private static Guid? ResolveAuthenticatedUserId(
        IClaimService claimService,
        ISessionService sessionService)
    {
        var externalUserId = claimService.GetExternalUserId();
        if (string.IsNullOrWhiteSpace(externalUserId))
            return null;

        var userId = sessionService.GetUserId();
        return userId == Guid.Empty ? null : userId;
    }

    private static string ComputeAnonymousUserPrint(string ip, string userAgent)
    {
        using var sha = SHA256.Create();
        var payload = $"{ip}|{userAgent}";
        var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var builder = new StringBuilder(hash.Length * 2);
        foreach (var item in hash) builder.Append(item.ToString("x2"));

        return builder.ToString();
    }
}

public readonly record struct ThreadActivityUserIdentity(
    string UserPrint,
    string Ip,
    string UserAgent,
    Guid? AuthenticatedUserId)
{
    public bool IsAuthenticated => AuthenticatedUserId.HasValue;
}
