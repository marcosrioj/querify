using MediatR;

namespace Querify.QnA.Portal.Business.Tag.Commands.DeleteTag;

public sealed class TagsDeleteTagCommand : IRequest
{
    public required Guid Id { get; set; }
}