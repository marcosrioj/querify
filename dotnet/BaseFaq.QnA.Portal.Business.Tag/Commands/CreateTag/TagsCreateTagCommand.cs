using BaseFaq.Models.QnA.Dtos.Tag;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.Tag.Commands.CreateTag;

public sealed class TagsCreateTagCommand : IRequest<Guid>
{
    public required TagCreateRequestDto Request { get; set; }
}