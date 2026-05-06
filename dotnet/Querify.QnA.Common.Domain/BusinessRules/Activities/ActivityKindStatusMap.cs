using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Models.QnA.Enums;
using System.Net;

namespace Querify.QnA.Common.Domain.BusinessRules.Activities;

public static class ActivityKindStatusMap
{
    public static ActivityKind ForQuestionStatus(QuestionStatus status)
    {
        return status switch
        {
            QuestionStatus.Draft => ActivityKind.QuestionDraft,
            QuestionStatus.Active => ActivityKind.QuestionActive,
            QuestionStatus.Archived => ActivityKind.QuestionArchived,
            _ => throw new ApiErrorException(
                "Unsupported question status.",
                (int)HttpStatusCode.UnprocessableEntity)
        };
    }

    public static ActivityKind ForAnswerStatus(AnswerStatus status)
    {
        return status switch
        {
            AnswerStatus.Draft => ActivityKind.AnswerDraft,
            AnswerStatus.Active => ActivityKind.AnswerActive,
            AnswerStatus.Archived => ActivityKind.AnswerArchived,
            _ => throw new ApiErrorException(
                "Unsupported answer status.",
                (int)HttpStatusCode.UnprocessableEntity)
        };
    }
}
