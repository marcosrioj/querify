using BaseFaq.Models.QnA.Dtos.Answer;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.Answer.Queries.GetAnswer;

public sealed class AnswersGetAnswerQuery : IRequest<AnswerDto>
{
    public Guid Id { get; set; }
}