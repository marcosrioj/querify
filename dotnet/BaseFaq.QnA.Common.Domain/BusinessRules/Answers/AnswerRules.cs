using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Domain.BusinessRules.Sources;
using BaseFaq.QnA.Common.Domain.Entities;

namespace BaseFaq.QnA.Common.Domain.BusinessRules.Answers;

public static class AnswerRules
{
    public static void SetSupportedStatus(Answer entity, AnswerStatus status)
    {
        EnsureSupportedStatus(status);
        entity.Status = status;
    }

    public static void EnsureSupportedStatus(AnswerStatus status)
    {
        if (status is AnswerStatus.Draft or AnswerStatus.Active or AnswerStatus.Archived)
            return;

        throw new ApiErrorException(
            "Unsupported answer status.",
            (int)HttpStatusCode.UnprocessableEntity);
    }

    public static void EnsureVisibilityAllowed(Answer entity, VisibilityScope visibility)
    {
        if (visibility is not VisibilityScope.Public) return;

        if (entity.Status is not AnswerStatus.Active)
            throw new ApiErrorException(
                "Only active answers can be exposed publicly.",
                (int)HttpStatusCode.UnprocessableEntity);

        foreach (var sourceLink in entity.Sources)
            SourceRules.EnsureReferenceSupportsPublicVisibility(visibility, sourceLink.Source, sourceLink.Role);
    }

    public static void Activate(Answer entity)
    {
        entity.Status = AnswerStatus.Active;
    }

    public static void Archive(Answer entity)
    {
        entity.Status = AnswerStatus.Archived;
        entity.Visibility = VisibilityScope.Internal;
    }

    public static AnswerSourceLink CreateSourceLink(
        Answer answer,
        Source source,
        SourceRole role,
        int order,
        Guid tenantId,
        string userId)
    {
        SourceRules.EnsureReferenceSupportsPublicVisibility(answer.Visibility, source, role);

        return new AnswerSourceLink
        {
            TenantId = tenantId,
            AnswerId = answer.Id,
            Answer = answer,
            SourceId = source.Id,
            Source = source,
            Role = role,
            Order = order,
            CreatedBy = userId,
            UpdatedBy = userId
        };
    }
}
