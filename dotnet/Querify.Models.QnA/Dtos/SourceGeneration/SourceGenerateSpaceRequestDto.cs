using Querify.Models.QnA.Enums;

namespace Querify.Models.QnA.Dtos.SourceGeneration;

public sealed class SourceGenerateSpaceRequestDto
{
    public required string SpaceName { get; set; }
    public string? SpaceSlug { get; set; }
    public string Language { get; set; } = "en-US";
    public VisibilityScope Visibility { get; set; } = VisibilityScope.Internal;
    public SpaceStatus Status { get; set; } = SpaceStatus.Draft;
    public bool AcceptsQuestions { get; set; } = true;
    public bool AcceptsAnswers { get; set; } = true;
    public string? ExtractionGoal { get; set; }
    public int MaxTopLevelQuestions { get; set; } = 3;
    public int MaxFollowUpDepth { get; set; } = 1;
    public int MaxAnswersPerQuestion { get; set; } = 1;
    public bool IncludeFollowUpQuestions { get; set; } = true;
    public SourceGenerationTagMode TagGenerationMode { get; set; } = SourceGenerationTagMode.CreateAndAttach;
    public SourceRole SourceRole { get; set; } = SourceRole.Origin;
    public bool RequireEveryAnswerToCiteSource { get; set; } = true;
    public string? ContentHint { get; set; }
}
