using Querify.Models.QnA.Dtos.Tag;
using MediatR;

namespace Querify.QnA.Portal.Business.Tag.Commands.CreateTag;

public sealed class TagsCreateTagCommand : IRequest<Guid>
{
    public required TagCreateRequestDto Request { get; set; }
}