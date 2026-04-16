using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.QnA.Dtos.QuestionSpace;
using MediatR;

namespace BaseFaq.QnA.Public.Business.QuestionSpace.Queries.GetQuestionSpaceByKey;

public sealed class QuestionSpacesGetQuestionSpaceByKeyQuery : IRequest<QuestionSpaceDto>
{
    public required string Key { get; set; }
}
