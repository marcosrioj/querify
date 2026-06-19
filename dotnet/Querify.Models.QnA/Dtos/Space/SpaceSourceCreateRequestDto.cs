using Querify.Models.QnA.Enums;

namespace Querify.Models.QnA.Dtos.Space;

public class SpaceSourceCreateRequestDto
{
    public required Guid SpaceId { get; set; }
    public required Guid SourceId { get; set; }
    public SourceRole Role { get; set; } = SourceRole.Reference;
}
