using MediatR;

namespace BaseFaq.Faq.Portal.Business.Tag.Commands.UpdateTag;

public sealed class TagsUpdateTagCommand : IRequest
{
    public required Guid Id { get; set; }
    public required string Value { get; set; }
}