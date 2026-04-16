using MediatR;

namespace BaseFaq.QnA.Portal.Business.Tag.Commands.DeleteTag;

public sealed class TagsDeleteTagCommand : IRequest
{
    public required Guid Id { get; set; }
}