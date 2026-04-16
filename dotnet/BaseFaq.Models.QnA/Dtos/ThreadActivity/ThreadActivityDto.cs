using BaseFaq.Models.QnA.Enums;

namespace BaseFaq.Models.QnA.Dtos.ThreadActivity;

public class ThreadActivityDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid QuestionId { get; set; }
    public Guid? AnswerId { get; set; }
    public ActivityKind Kind { get; set; }
    public ActorKind ActorKind { get; set; }
    public string? ActorLabel { get; set; }
    public string? Notes { get; set; }
    public string? MetadataJson { get; set; }
    public DateTime OccurredAtUtc { get; set; }
}
