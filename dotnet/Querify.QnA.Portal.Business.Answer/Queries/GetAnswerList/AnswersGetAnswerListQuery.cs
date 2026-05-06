using Querify.Models.Common.Dtos;
using Querify.Models.QnA.Dtos.Answer;
using MediatR;

namespace Querify.QnA.Portal.Business.Answer.Queries.GetAnswerList;

public sealed class AnswersGetAnswerListQuery : IRequest<PagedResultDto<AnswerDto>>
{
    public required AnswerGetAllRequestDto Request { get; set; }
}