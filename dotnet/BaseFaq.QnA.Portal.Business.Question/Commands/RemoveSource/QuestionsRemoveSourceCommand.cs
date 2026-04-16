using MediatR;

namespace BaseFaq.QnA.Portal.Business.Question.Commands.RemoveSource;

public sealed class QuestionsRemoveSourceCommand : IRequest
{
    public Guid QuestionId { get; set; }
    public Guid SourceLinkId { get; set; }
}