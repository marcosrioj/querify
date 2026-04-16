using Microsoft.AspNetCore.Http;

namespace BaseFaq.QnA.Common.Helper.Activities;

public static class ActivityRequestInfo
{
    public static string GetRequiredIp(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        var ip = string.IsNullOrWhiteSpace(forwardedFor)
            ? httpContext.Connection.RemoteIpAddress?.ToString()
            : forwardedFor.Split(',')[0].Trim();

        return !string.IsNullOrWhiteSpace(ip)
            ? ip
            : throw new InvalidOperationException("Activity identity requires a request IP address.");
    }

    public static string GetRequiredUserAgent(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        var userAgent = httpContext.Request.Headers.UserAgent.ToString();
        return !string.IsNullOrWhiteSpace(userAgent)
            ? userAgent
            : throw new InvalidOperationException("Activity identity requires a request user agent.");
    }
}
