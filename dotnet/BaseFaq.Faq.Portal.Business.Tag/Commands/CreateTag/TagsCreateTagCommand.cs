using MediatR;

namespace BaseFaq.Faq.Portal.Business.Tag.Commands.CreateTag;

public sealed class TagsCreateTagCommand : IRequest<Guid>
{
    public required string Value { get; set; }
}