using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Domain.Entities;

namespace BaseFaq.QnA.Common.Domain.BusinessRules.Spaces;

public static class SpaceRules
{
    public static void EnsureAcceptsQuestions(Space space)
    {
        if (space.AcceptsQuestions)
            return;

        throw new ApiErrorException(
            "This space is not accepting questions.",
            (int)HttpStatusCode.UnprocessableEntity);
    }

    public static void EnsureAcceptsAnswers(Space space)
    {
        if (space.AcceptsAnswers)
            return;

        throw new ApiErrorException(
            "This space is not accepting answers.",
            (int)HttpStatusCode.UnprocessableEntity);
    }

    public static void EnsureVisibilityAllowed(Space entity, VisibilityScope visibility)
    {
        if (visibility is not VisibilityScope.Public) return;

        if (entity.Status is not SpaceStatus.Active)
            throw new ApiErrorException(
                "Only active spaces can be exposed publicly.",
                (int)HttpStatusCode.UnprocessableEntity);
    }

    public static SpaceTag EnsureTagLink(Space space, Tag tag, Guid tenantId, string userId)
    {
        var existingLink = space.Tags.SingleOrDefault(link => link.TagId == tag.Id);
        if (existingLink is not null)
            return existingLink;

        var link = new SpaceTag
        {
            TenantId = tenantId,
            SpaceId = space.Id,
            Space = space,
            TagId = tag.Id,
            Tag = tag,
            CreatedBy = userId,
            UpdatedBy = userId
        };

        space.Tags.Add(link);
        return link;
    }

    public static SpaceSource EnsureSourceLink(Space space, Source source, Guid tenantId, string userId)
    {
        var existingLink = space.Sources.SingleOrDefault(link => link.SourceId == source.Id);
        if (existingLink is not null)
            return existingLink;

        var link = new SpaceSource
        {
            TenantId = tenantId,
            SpaceId = space.Id,
            Space = space,
            SourceId = source.Id,
            Source = source,
            CreatedBy = userId,
            UpdatedBy = userId
        };

        space.Sources.Add(link);
        return link;
    }
}
