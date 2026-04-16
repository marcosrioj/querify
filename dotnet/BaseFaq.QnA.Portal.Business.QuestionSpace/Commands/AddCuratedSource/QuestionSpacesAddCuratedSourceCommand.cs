using BaseFaq.Models.QnA.Dtos.QuestionSpace;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.QuestionSpace.Commands;

public sealed class QuestionSpacesAddCuratedSourceCommand : IRequest<Guid>
{
    public required QuestionSpaceSourceCreateRequestDto Request { get; set; }
}
