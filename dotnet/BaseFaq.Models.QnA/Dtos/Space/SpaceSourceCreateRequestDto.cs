namespace BaseFaq.Models.QnA.Dtos.Space;

public class SpaceSourceCreateRequestDto
{
    public required Guid SpaceId { get; set; }
    public required Guid SourceId { get; set; }
}
