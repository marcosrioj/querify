using Querify.Models.QnA.Enums;

namespace Querify.Models.QnA.Dtos.Search;

public class QnASearchResultDto
{
    public required Guid TenantId { get; set; }
    public required Guid QuestionId { get; set; }
    public required Guid SpaceId { get; set; }
    public required string SpaceSlug { get; set; }
    public required string SpaceName { get; set; }
    public required string Title { get; set; }
    public string? Summary { get; set; }
    public required QuestionStatus Status { get; set; }
    public required VisibilityScope Visibility { get; set; }
    public Guid? AcceptedAnswerId { get; set; }
    public string? AcceptedAnswerHeadline { get; set; }
    public string? AcceptedAnswerBodyPreview { get; set; }
    public required int AnswerCount { get; set; }
    public IReadOnlyList<string> Tags { get; set; } = [];
    public DateTime? LastActivityAtUtc { get; set; }
    public DateTime? LastUpdatedAtUtc { get; set; }
    public required bool MatchedQuestion { get; set; }
    public required bool MatchedAnswer { get; set; }
    public required bool MatchedSpace { get; set; }
    public required bool MatchedTag { get; set; }
}
