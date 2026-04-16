using BaseFaq.Models.QnA.Enums;

namespace BaseFaq.Models.QnA.Dtos.Activity;

public class ActivityDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid QuestionId { get; set; }
    public Guid? AnswerId { get; set; }
    public ActivityKind Kind { get; set; }
    public ActorKind ActorKind { get; set; }
    public string? ActorLabel { get; set; }
    public string UserPrint { get; set; } = string.Empty;
    public string Ip { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? MetadataJson { get; set; }
    public DateTime OccurredAtUtc { get; set; }
}
