namespace BaseFaq.Models.QnA.Dtos.Tag;

public class TagDto
{
    public required Guid Id { get; set; }
    public required Guid TenantId { get; set; }
    public required string Name { get; set; } = string.Empty;
}
