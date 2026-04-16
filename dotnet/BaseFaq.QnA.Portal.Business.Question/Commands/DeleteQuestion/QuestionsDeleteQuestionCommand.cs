using BaseFaq.Models.QnA.Dtos.Question;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.Question.Commands;

public sealed class QuestionsDeleteQuestionCommand : IRequest
{
    public Guid Id { get; set; }
}
