using BaseFaq.Models.QnA.Enums;

namespace BaseFaq.Models.QnA.Dtos.Question;

public class QuestionDto
{
    public required Guid Id { get; set; }
    public required Guid TenantId { get; set; }
    public required Guid SpaceId { get; set; }
    public required string SpaceSlug { get; set; }
    public required string Title { get; set; }
    public string? Summary { get; set; }
    public string? ContextNote { get; set; }
    public required QuestionStatus Status { get; set; }
    public required VisibilityScope Visibility { get; set; }
    public required ChannelKind OriginChannel { get; set; }
    public required int AiConfidenceScore { get; set; }
    public required int FeedbackScore { get; set; }
    public required int Sort { get; set; }
    public Guid? AcceptedAnswerId { get; set; }
    public DateTime? LastActivityAtUtc { get; set; }
    public DateTime? LastUpdatedAtUtc { get; set; }
}
