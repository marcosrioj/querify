using Querify.Models.QnA.Dtos.Answer;
using MediatR;

namespace Querify.QnA.Portal.Business.Answer.Queries.GetAnswer;

public sealed class AnswersGetAnswerQuery : IRequest<AnswerDto>
{
    public required Guid Id { get; set; }
}