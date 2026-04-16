using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.QnA.Dtos.QuestionSpace;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.QuestionSpace.Queries.GetQuestionSpaceList;

public sealed class QuestionSpacesGetQuestionSpaceListQuery : IRequest<PagedResultDto<QuestionSpaceDto>>
{
    public required QuestionSpaceGetAllRequestDto Request { get; set; }
}