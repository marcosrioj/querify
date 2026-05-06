using System.Net;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Models.QnA.Enums;
using Querify.QnA.Common.Domain.Entities;

namespace Querify.QnA.Common.Domain.BusinessRules.Sources;

public static class SourceRules
{
    public static void EnsureVisibilityAllowed(Source entity, VisibilityScope visibility)
    {
        if (visibility is not VisibilityScope.Public) return;

        if (entity.Kind is SourceKind.InternalNote)
            throw new ApiErrorException(
                "Internal notes cannot be exposed publicly.",
                (int)HttpStatusCode.UnprocessableEntity);

        if (entity.LastVerifiedAtUtc is null)
            throw new ApiErrorException(
                "Sources must be verified before public exposure.",
                (int)HttpStatusCode.UnprocessableEntity);
    }

    public static void EnsureReferenceSupportsPublicVisibility(
        VisibilityScope ownerVisibility,
        Source source,
        SourceRole role)
    {
        if (ownerVisibility is not VisibilityScope.Public) return;

        if (role is SourceRole.Reference &&
            source.Visibility is not VisibilityScope.Public)
            throw new ApiErrorException(
                "Public references require a publicly visible source.",
                (int)HttpStatusCode.UnprocessableEntity);
    }
}
