using Querify.Models.QnA.Enums;

namespace Querify.Models.QnA.Dtos.Activity;

public class ActivityDto
{
    public required Guid Id { get; set; }
    public required Guid TenantId { get; set; }
    public required Guid QuestionId { get; set; }
    public string? QuestionTitle { get; set; }
    public Guid? AnswerId { get; set; }
    public string? AnswerHeadline { get; set; }
    public required ActivityKind Kind { get; set; }
    public required ActorKind ActorKind { get; set; }
    public string? ActorLabel { get; set; }
    public required string UserPrint { get; set; }
    public required string Ip { get; set; }
    public required string UserAgent { get; set; }
    public string? Notes { get; set; }
    public string? MetadataJson { get; set; }
    public required DateTime OccurredAtUtc { get; set; }
}
