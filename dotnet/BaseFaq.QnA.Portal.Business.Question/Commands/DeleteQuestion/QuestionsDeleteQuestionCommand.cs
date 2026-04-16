using MediatR;

namespace BaseFaq.QnA.Portal.Business.Question.Commands.DeleteQuestion;

public sealed class QuestionsDeleteQuestionCommand : IRequest
{
    public required Guid Id { get; set; }
}