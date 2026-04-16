using BaseFaq.Models.QnA.Dtos.QuestionSpace;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.QuestionSpace.Commands;

public sealed class QuestionSpacesRemoveCuratedSourceCommand : IRequest
{
    public Guid QuestionSpaceId { get; set; }
    public Guid KnowledgeSourceId { get; set; }
}
