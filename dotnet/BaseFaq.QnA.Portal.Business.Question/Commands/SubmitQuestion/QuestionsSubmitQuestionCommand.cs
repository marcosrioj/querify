using MediatR;

namespace BaseFaq.QnA.Portal.Business.Question.Commands.SubmitQuestion;

public sealed class QuestionsSubmitQuestionCommand : IRequest<Guid>
{
    public required Guid Id { get; set; }
}