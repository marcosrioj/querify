using System.Net;
using System.Security.Cryptography;
using System.Text;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using Microsoft.AspNetCore.Http;

namespace BaseFaq.Faq.Portal.Business.Vote.Helpers;

public static class VoteRequestContext
{
    public static VoteRequestIdentity GetIdentity(
        ISessionService sessionService,
        IHttpContextAccessor httpContextAccessor)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext is null)
        {
            throw new ApiErrorException(
                "HttpContext is missing from the current request.",
                errorCode: (int)HttpStatusCode.Unauthorized);
        }

        var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        var ip = string.IsNullOrWhiteSpace(forwardedFor)
            ? httpContext.Connection.RemoteIpAddress?.ToString()
            : forwardedFor.Split(',')[0].Trim();
        ip ??= string.Empty;

        var userAgent = httpContext.Request.Headers.UserAgent.ToString();
        var userPrint = httpContext.User?.Identity?.IsAuthenticated == true
            ? sessionService.GetUserId().ToString()
            : ComputeUserPrint(ip, userAgent);

        return new VoteRequestIdentity(userPrint, ip, userAgent);
    }

    private static string ComputeUserPrint(string ip, string userAgent)
    {
        using var sha = SHA256.Create();
        var payload = $"{ip}|{userAgent}";
        var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var builder = new StringBuilder(hash.Length * 2);
        foreach (var item in hash)
        {
            builder.Append(item.ToString("x2"));
        }

        return builder.ToString();
    }
}

public readonly record struct VoteRequestIdentity(string UserPrint, string Ip, string UserAgent);
