using MediatR;

namespace Querify.QnA.Portal.Business.Question.Commands.RemoveSource;

public sealed class QuestionsRemoveSourceCommand : IRequest
{
    public required Guid QuestionId { get; set; }
    public required Guid SourceLinkId { get; set; }
}