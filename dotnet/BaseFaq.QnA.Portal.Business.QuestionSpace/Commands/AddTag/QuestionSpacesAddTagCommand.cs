using BaseFaq.Models.QnA.Dtos.QuestionSpace;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.QuestionSpace.Commands.AddTag;

public sealed class QuestionSpacesAddTagCommand : IRequest<Guid>
{
    public required QuestionSpaceTagCreateRequestDto Request { get; set; }
}