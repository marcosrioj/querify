using MediatR;

namespace BaseFaq.QnA.Portal.Business.Question.Commands.ApproveQuestion;

public sealed class QuestionsApproveQuestionCommand : IRequest<Guid>
{
    public required Guid Id { get; set; }
}