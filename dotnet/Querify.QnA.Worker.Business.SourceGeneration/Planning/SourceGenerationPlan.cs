namespace Querify.QnA.Worker.Business.SourceGeneration.Planning;

internal sealed record SourceGenerationPlan(
    SourceGenerationSpacePlan Space,
    IReadOnlyList<string> Tags,
    IReadOnlyList<SourceGenerationQuestionPlan> Questions,
    IReadOnlyList<string> Warnings);

internal sealed record SourceGenerationSpacePlan(
    string Name,
    string? Slug,
    string Language);

internal sealed record SourceGenerationQuestionPlan(
    string TempId,
    string Title,
    string? Summary,
    string? ContextNote,
    string? ParentAnswerTempId,
    IReadOnlyList<SourceGenerationAnswerPlan> Answers);

internal sealed record SourceGenerationAnswerPlan(
    string TempId,
    string Headline,
    string Body,
    string? ContextNote);
