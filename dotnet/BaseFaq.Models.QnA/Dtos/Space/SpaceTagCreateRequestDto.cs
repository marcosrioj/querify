namespace BaseFaq.Models.QnA.Dtos.Space;

public class SpaceTagCreateRequestDto
{
    public required Guid SpaceId { get; set; }
    public required Guid TagId { get; set; }
}
