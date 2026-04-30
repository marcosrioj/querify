using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.QnA.Enums;
using Microsoft.AspNetCore.Http;

namespace BaseFaq.QnA.Common.Domain.BusinessRules.Activities;

public static class ActivityActorResolver
{
    public static ActivityActor ResolvePortalActor(
        ISessionService sessionService,
        IHttpContextAccessor httpContextAccessor,
        ActorKind actorKind)
    {
        var userId = sessionService.GetUserId();
        var httpContext = GetRequiredHttpContext(httpContextAccessor);
        var identity = ActivityIdentityResolver.ResolveActivityIdentity(
            userId.ToString("D"),
            ActivityRequestInfo.GetRequiredIp(httpContext),
            ActivityRequestInfo.GetRequiredUserAgent(httpContext));

        return new ActivityActor(
            actorKind,
            identity.UserPrint,
            identity.Ip,
            identity.UserAgent,
            userId,
            sessionService.GetUserName(),
            false);
    }

    public static ActivityActor ResolvePublicActor(
        ISessionService sessionService,
        IClaimService claimService,
        IHttpContextAccessor httpContextAccessor,
        ActorKind actorKind)
    {
        var httpContext = GetRequiredHttpContext(httpContextAccessor);
        var identity = ActivityIdentityResolver.ResolveActivityIdentity(
            sessionService,
            ActivityRequestInfo.GetRequiredIp(httpContext),
            ActivityRequestInfo.GetRequiredUserAgent(httpContext),
            claimService.GetExternalUserId());

        return new ActivityActor(
            actorKind,
            identity.UserPrint,
            identity.Ip,
            identity.UserAgent,
            null,
            null,
            true);
    }

    private static HttpContext GetRequiredHttpContext(IHttpContextAccessor httpContextAccessor)
    {
        return httpContextAccessor.HttpContext
               ?? throw new ApiErrorException(
                   "HttpContext is missing from the current request.",
                   (int)HttpStatusCode.Unauthorized);
    }
}
