using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Domain.BusinessRules.Sources;
using BaseFaq.QnA.Common.Domain.Entities;

namespace BaseFaq.QnA.Common.Domain.BusinessRules.Questions;

public static class QuestionRules
{
    public static void EnsureSupportedStatus(QuestionStatus status)
    {
        if (status is QuestionStatus.Draft or QuestionStatus.Active or QuestionStatus.Archived)
            return;

        throw new ApiErrorException(
            "Unsupported question status.",
            (int)HttpStatusCode.UnprocessableEntity);
    }

    public static void EnsureVisibilityAllowed(Question entity, VisibilityScope visibility)
    {
        if (visibility is not VisibilityScope.Public) return;

        if (entity.Status is not QuestionStatus.Active)
            throw new ApiErrorException(
                "Only active questions can be exposed publicly.",
                (int)HttpStatusCode.UnprocessableEntity);

        foreach (var sourceLink in entity.Sources)
            SourceRules.EnsureReferenceSupportsPublicVisibility(visibility, sourceLink.Source, sourceLink.Role);

        if (entity.AcceptedAnswer is not null &&
            entity.AcceptedAnswer.Visibility is not VisibilityScope.Public)
            throw new ApiErrorException(
                "Public questions require a publicly visible accepted answer.",
                (int)HttpStatusCode.UnprocessableEntity);
    }

    public static void ApplyAcceptedAnswer(Question question, Answer answer)
    {
        if (answer.QuestionId != question.Id)
            throw new ApiErrorException(
                $"Accepted answer '{answer.Id}' belongs to a different question.",
                (int)HttpStatusCode.UnprocessableEntity);

        if (answer.Status is not AnswerStatus.Active)
            throw new ApiErrorException(
                "Only active answers can be accepted.",
                (int)HttpStatusCode.UnprocessableEntity);

        if (question.Visibility is VisibilityScope.Public &&
            answer.Visibility is not VisibilityScope.Public)
            throw new ApiErrorException(
                "Public questions cannot accept non-public answers.",
                (int)HttpStatusCode.UnprocessableEntity);

        question.AcceptedAnswerId = answer.Id;
        question.AcceptedAnswer = answer;

        if (question.Status is QuestionStatus.Draft)
            question.Status = QuestionStatus.Active;
    }

    public static void ClearAcceptedAnswer(Question question)
    {
        question.AcceptedAnswerId = null;
        question.AcceptedAnswer = null;
    }

    public static QuestionSourceLink CreateSourceLink(
        Question question,
        Source source,
        SourceRole role,
        int order,
        Guid tenantId,
        string userId)
    {
        SourceRules.EnsureReferenceSupportsPublicVisibility(question.Visibility, source, role);

        return new QuestionSourceLink
        {
            TenantId = tenantId,
            QuestionId = question.Id,
            Question = question,
            SourceId = source.Id,
            Source = source,
            Role = role,
            Order = order,
            CreatedBy = userId,
            UpdatedBy = userId
        };
    }

    public static QuestionTag EnsureTagLink(Question question, Tag tag, Guid tenantId, string userId)
    {
        var existingLink = question.Tags.SingleOrDefault(link => link.TagId == tag.Id);
        if (existingLink is not null)
            return existingLink;

        var link = new QuestionTag
        {
            TenantId = tenantId,
            QuestionId = question.Id,
            Question = question,
            TagId = tag.Id,
            Tag = tag,
            CreatedBy = userId,
            UpdatedBy = userId
        };

        question.Tags.Add(link);
        return link;
    }
}
