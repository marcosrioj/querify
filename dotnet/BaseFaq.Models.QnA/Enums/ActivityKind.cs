namespace BaseFaq.Models.QnA.Enums;

public enum ActivityKind
{
    QuestionCreated = 1,
    QuestionUpdated = 2,
    QuestionSubmitted = 3,
    QuestionApproved = 4,
    QuestionRejected = 5,
    QuestionMarkedDuplicate = 6,
    QuestionEscalated = 7,
    AnswerCreated = 8,
    AnswerUpdated = 9,
    AnswerPublished = 10,
    AnswerAccepted = 11,
    AnswerValidated = 12,
    AnswerRejected = 13,
    FeedbackReceived = 14,
    VoteReceived = 15
}
