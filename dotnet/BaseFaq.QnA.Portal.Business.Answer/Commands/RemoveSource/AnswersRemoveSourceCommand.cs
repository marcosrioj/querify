using BaseFaq.Models.QnA.Dtos.Answer;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.Answer.Commands;

public sealed class AnswersRemoveSourceCommand : IRequest
{
    public Guid AnswerId { get; set; }
    public Guid SourceLinkId { get; set; }
}
