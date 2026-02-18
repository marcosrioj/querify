using MediatR;

namespace BaseFaq.Faq.Portal.Business.Tag.Commands.DeleteTag;

public sealed class TagsDeleteTagCommand : IRequest
{
    public required Guid Id { get; set; }
}