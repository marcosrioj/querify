using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.QnA.Dtos.QuestionSpace;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.QuestionSpace.Queries.GetQuestionSpace;

public sealed class QuestionSpacesGetQuestionSpaceQuery : IRequest<QuestionSpaceDetailDto>
{
    public Guid Id { get; set; }
}
