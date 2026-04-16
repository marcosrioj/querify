using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using Microsoft.AspNetCore.Http;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Identity;

public static class ActivityUserPrint
{
    private const string ExternalUserIdClaimType = "sub";
    private const string UnknownIp = "unknown";
    private const string UnknownUserAgent = "unknown";

    public static ActivityUserIdentity ResolveCurrent(
        HttpContext httpContext,
        IClaimService claimService,
        ISessionService sessionService)
    {
        ArgumentNullException.ThrowIfNull(claimService);
        return ResolveCurrent(httpContext, sessionService, claimService.GetExternalUserId());
    }

    public static ActivityUserIdentity ResolveCurrent(
        HttpContext httpContext,
        ISessionService sessionService)
    {
        return ResolveCurrent(httpContext, sessionService, ResolveExternalUserId(httpContext.User));
    }

    public static ActivityUserIdentity ResolveForPersistence(
        HttpContext? httpContext,
        ISessionService sessionService,
        string? explicitUserPrint,
        string? explicitIp,
        string? explicitUserAgent,
        params string?[] fallbackLabels)
    {
        ArgumentNullException.ThrowIfNull(sessionService);

        if (httpContext is not null)
        {
            var current = ResolveCurrent(httpContext, sessionService);
            return new ActivityUserIdentity(
                string.IsNullOrWhiteSpace(explicitUserPrint) ? current.UserPrint : explicitUserPrint,
                string.IsNullOrWhiteSpace(explicitIp) ? current.Ip : explicitIp,
                string.IsNullOrWhiteSpace(explicitUserAgent) ? current.UserAgent : explicitUserAgent,
                current.AuthenticatedUserId);
        }

        var fallbackUserPrint = FirstNonEmpty(explicitUserPrint, fallbackLabels) ?? "system";
        Guid? authenticatedUserId = Guid.TryParse(fallbackUserPrint, out var parsedUserId)
            ? parsedUserId
            : null;

        return new ActivityUserIdentity(
            fallbackUserPrint,
            FirstNonEmpty(explicitIp, [UnknownIp])!,
            FirstNonEmpty(explicitUserAgent, [UnknownUserAgent])!,
            authenticatedUserId);
    }

    private static ActivityUserIdentity ResolveCurrent(
        HttpContext httpContext,
        ISessionService sessionService,
        string? externalUserId)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(sessionService);

        var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        var ip = string.IsNullOrWhiteSpace(forwardedFor)
            ? httpContext.Connection.RemoteIpAddress?.ToString()
            : forwardedFor.Split(',')[0].Trim();
        ip = string.IsNullOrWhiteSpace(ip) ? UnknownIp : ip;

        var userAgent = httpContext.Request.Headers.UserAgent.ToString();
        userAgent = string.IsNullOrWhiteSpace(userAgent) ? UnknownUserAgent : userAgent;
        var authenticatedUserId = ResolveAuthenticatedUserId(externalUserId, sessionService);
        var userPrint = authenticatedUserId?.ToString("D") ?? ComputeAnonymousUserPrint(ip, userAgent);

        return new ActivityUserIdentity(userPrint, ip, userAgent, authenticatedUserId);
    }

    public static string? ResolveStored(string? explicitUserPrint, string? metadataUserPrint)
    {
        if (!string.IsNullOrWhiteSpace(explicitUserPrint))
            return explicitUserPrint;

        return string.IsNullOrWhiteSpace(metadataUserPrint) ? null : metadataUserPrint;
    }

    private static Guid? ResolveAuthenticatedUserId(
        string? externalUserId,
        ISessionService sessionService)
    {
        if (string.IsNullOrWhiteSpace(externalUserId))
            return null;

        var userId = sessionService.GetUserId();
        return userId == Guid.Empty ? null : userId;
    }

    private static string? ResolveExternalUserId(ClaimsPrincipal? user)
    {
        return user?.FindFirstValue(ExternalUserIdClaimType);
    }

    private static string? FirstNonEmpty(string? first, IEnumerable<string?> rest)
    {
        if (!string.IsNullOrWhiteSpace(first))
            return first;

        foreach (var value in rest)
        {
            if (!string.IsNullOrWhiteSpace(value))
                return value;
        }

        return null;
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

public readonly record struct ActivityUserIdentity(
    string UserPrint,
    string Ip,
    string UserAgent,
    Guid? AuthenticatedUserId)
{
    public bool IsAuthenticated => AuthenticatedUserId.HasValue;
}
