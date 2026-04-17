using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.QnA.Dtos.Answer;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.Answer.Queries.GetAnswerList;

public sealed class AnswersGetAnswerListQuery : IRequest<PagedResultDto<AnswerDto>>
{
    public required AnswerGetAllRequestDto Request { get; set; }
}