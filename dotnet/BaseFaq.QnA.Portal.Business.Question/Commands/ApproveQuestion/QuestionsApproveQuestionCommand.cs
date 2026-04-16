using BaseFaq.Models.QnA.Dtos.Question;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.Question.Commands.ApproveQuestion;

public sealed class QuestionsApproveQuestionCommand : IRequest<Guid>
{
    public Guid Id { get; set; }
}
