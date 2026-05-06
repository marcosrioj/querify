using MediatR;

namespace Querify.QnA.Portal.Business.Answer.Commands.RemoveSource;

public sealed class AnswersRemoveSourceCommand : IRequest
{
    public required Guid AnswerId { get; set; }
    public required Guid SourceLinkId { get; set; }
}