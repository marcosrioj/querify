using MediatR;

namespace BaseFaq.QnA.Portal.Business.Tag.Commands.DeleteTag;

public sealed class TagsDeleteTagCommand : IRequest
{
    public Guid Id { get; set; }
}