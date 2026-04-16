using System.Security.Cryptography;
using System.Text;
using BaseFaq.Common.Infrastructure.Core.Abstractions;

namespace BaseFaq.QnA.Common.Helper.Activities;

public static class ActivityIdentityResolver
{
    public static ActivityUserIdentity ResolveActivityIdentity(
        string explicitUserPrint,
        string ip,
        string userAgent)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(explicitUserPrint);
        ArgumentException.ThrowIfNullOrWhiteSpace(ip);
        ArgumentException.ThrowIfNullOrWhiteSpace(userAgent);

        Guid? authenticatedUserId = Guid.TryParse(explicitUserPrint, out var parsedUserId)
            ? parsedUserId
            : null;

        return new ActivityUserIdentity(explicitUserPrint, ip, userAgent, authenticatedUserId);
    }

    public static ActivityUserIdentity ResolveActivityIdentity(
        ISessionService sessionService,
        string ip,
        string userAgent,
        string? externalUserId)
    {
        ArgumentNullException.ThrowIfNull(sessionService);
        ArgumentException.ThrowIfNullOrWhiteSpace(ip);
        ArgumentException.ThrowIfNullOrWhiteSpace(userAgent);

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

    private static string ComputeAnonymousUserPrint(string ip, string userAgent)
    {
        using var sha = SHA256.Create();
        var payload = $"{ip}|{userAgent}";
        var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var builder = new StringBuilder(hash.Length * 2);

        foreach (var item in hash)
            builder.Append(item.ToString("x2"));

        return builder.ToString();
    }
}