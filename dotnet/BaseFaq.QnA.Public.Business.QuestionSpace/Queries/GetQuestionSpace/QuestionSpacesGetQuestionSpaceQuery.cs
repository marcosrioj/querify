using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.QnA.Dtos.QuestionSpace;
using MediatR;

namespace BaseFaq.QnA.Public.Business.QuestionSpace.Queries;

public sealed class QuestionSpacesGetQuestionSpaceQuery : IRequest<QuestionSpaceDto>
{
    public Guid Id { get; set; }
}
