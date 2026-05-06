using Querify.Models.QnA.Dtos.Tag;
using MediatR;

namespace Querify.QnA.Portal.Business.Tag.Queries.GetTag;

public sealed class TagsGetTagQuery : IRequest<TagDto>
{
    public required Guid Id { get; set; }
}