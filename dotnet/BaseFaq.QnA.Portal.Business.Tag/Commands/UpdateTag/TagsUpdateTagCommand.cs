using BaseFaq.Models.QnA.Dtos.Tag;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.Tag.Commands.UpdateTag;

public sealed class TagsUpdateTagCommand : IRequest<Guid>
{
    public required Guid Id { get; set; }
    public required TagUpdateRequestDto Request { get; set; }
}