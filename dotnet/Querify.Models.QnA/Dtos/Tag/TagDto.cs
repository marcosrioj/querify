namespace Querify.Models.QnA.Dtos.Tag;

public class TagDto
{
    public required Guid Id { get; set; }
    public required Guid TenantId { get; set; }
    public required string Name { get; set; }
    public int SpaceUsageCount { get; set; }
    public int QuestionUsageCount { get; set; }
    public DateTime? LastUpdatedAtUtc { get; set; }
}
