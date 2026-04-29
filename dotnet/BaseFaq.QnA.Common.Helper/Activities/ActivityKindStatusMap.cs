using BaseFaq.Models.QnA.Enums;

namespace BaseFaq.QnA.Common.Helper.Activities;

public static class ActivityKindStatusMap
{
    public static ActivityKind ForQuestionStatus(QuestionStatus status)
    {
        return status switch
        {
            QuestionStatus.Draft => ActivityKind.QuestionDraft,
            QuestionStatus.Active => ActivityKind.QuestionActive,
            QuestionStatus.Archived => ActivityKind.QuestionArchived,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unsupported question status.")
        };
    }

    public static ActivityKind ForAnswerStatus(AnswerStatus status)
    {
        return status switch
        {
            AnswerStatus.Draft => ActivityKind.AnswerDraft,
            AnswerStatus.Active => ActivityKind.AnswerActive,
            AnswerStatus.Archived => ActivityKind.AnswerArchived,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unsupported answer status.")
        };
    }
}
