namespace BaseFaq.Models.QnA.Dtos.Tag;

public class TagDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
}
