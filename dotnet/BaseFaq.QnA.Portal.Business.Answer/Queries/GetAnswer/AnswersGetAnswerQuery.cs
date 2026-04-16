using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.QnA.Dtos.Answer;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.Answer.Queries;

public sealed class AnswersGetAnswerQuery : IRequest<AnswerDto>
{
    public Guid Id { get; set; }
}
