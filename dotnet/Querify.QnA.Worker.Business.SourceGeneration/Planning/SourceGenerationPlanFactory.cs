using Querify.QnA.Common.Domain.Entities;
using Querify.Models.QnA.Enums;

namespace Querify.QnA.Worker.Business.SourceGeneration.Planning;

internal static class SourceGenerationPlanFactory
{
    public static SourceGenerationPlan Create(SourceGenerationRun run, Source source)
    {
        var sourceLabel = ResolveSourceLabel(source);
        var sourceLocator = string.IsNullOrWhiteSpace(source.Locator)
            ? source.StorageKey ?? "source content"
            : source.Locator;
        var questionCount = Math.Clamp(run.MaxTopLevelQuestions, 1, 12);
        var answerCount = Math.Clamp(run.MaxAnswersPerQuestion, 1, 3);
        var questions = new List<SourceGenerationQuestionPlan>();

        for (var index = 1; index <= questionCount; index++)
        {
            var questionTempId = $"q-{index}";
            var answers = BuildAnswers(run, sourceLabel, sourceLocator, questionTempId, answerCount);
            questions.Add(new SourceGenerationQuestionPlan(
                questionTempId,
                BuildQuestionTitle(index, sourceLabel),
                $"Draft question generated from {sourceLabel}.",
                BuildQuestionContext(run, source),
                null,
                answers));

            if (!run.IncludeFollowUpQuestions || run.MaxFollowUpDepth <= 0)
                continue;

            var parentAnswerTempId = answers[0].TempId;
            var followUpTempId = $"q-{index}-follow-up-1";
            questions.Add(new SourceGenerationQuestionPlan(
                followUpTempId,
                $"What evidence from {sourceLabel} supports this answer?",
                $"Follow-up question generated from {sourceLabel}.",
                BuildQuestionContext(run, source),
                parentAnswerTempId,
                BuildAnswers(run, sourceLabel, sourceLocator, followUpTempId, 1)));
        }

        var warnings = new List<string>
        {
            "Local MVP generation used source metadata and locator context only. Review generated draft content before activation."
        };

        if (run.MaxFollowUpDepth > 1)
            warnings.Add("Only the first follow-up depth is populated by the local MVP generator.");

        return new SourceGenerationPlan(
            new SourceGenerationSpacePlan(run.SpaceName, run.SpaceSlug, run.Language),
            BuildTags(run, source),
            questions,
            warnings);
    }

    private static IReadOnlyList<SourceGenerationAnswerPlan> BuildAnswers(
        SourceGenerationRun run,
        string sourceLabel,
        string sourceLocator,
        string questionTempId,
        int answerCount)
    {
        var answers = new List<SourceGenerationAnswerPlan>();

        for (var index = 1; index <= answerCount; index++)
        {
            answers.Add(new SourceGenerationAnswerPlan(
                $"{questionTempId}-a-{index}",
                index == 1 ? $"Review {sourceLabel}" : $"Additional review note {index}",
                BuildAnswerBody(run, sourceLabel, sourceLocator),
                "Generated as Draft/Internal and requires human review before activation."));
        }

        return answers;
    }

    private static string BuildQuestionTitle(int index, string sourceLabel)
    {
        return index switch
        {
            1 => $"What should users know about {sourceLabel}?",
            2 => $"When should {sourceLabel} be used?",
            3 => $"What review notes matter for {sourceLabel}?",
            _ => $"What is source item {index} from {sourceLabel}?"
        };
    }

    private static string BuildAnswerBody(SourceGenerationRun run, string sourceLabel, string sourceLocator)
    {
        var goal = string.IsNullOrWhiteSpace(run.ExtractionGoal)
            ? "No additional extraction goal was provided."
            : $"Extraction goal: {run.ExtractionGoal.Trim()}";
        var hint = string.IsNullOrWhiteSpace(run.ContentHint)
            ? "No content range hint was provided."
            : $"Content hint: {run.ContentHint.Trim()}";

        return
            $"This draft answer is grounded in source '{sourceLabel}' ({sourceLocator}). {goal} {hint} Review the source evidence before activating or exposing this answer.";
    }

    private static string BuildQuestionContext(SourceGenerationRun run, Source source)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(source.ContextNote))
            parts.Add(source.ContextNote.Trim());

        if (!string.IsNullOrWhiteSpace(run.ExtractionGoal))
            parts.Add(run.ExtractionGoal.Trim());

        if (!string.IsNullOrWhiteSpace(run.ContentHint))
            parts.Add(run.ContentHint.Trim());

        return parts.Count == 0
            ? "Generated from source metadata for operator review."
            : string.Join(" ", parts);
    }

    private static IReadOnlyList<string> BuildTags(SourceGenerationRun run, Source source)
    {
        if (run.TagGenerationMode is SourceGenerationTagMode.None)
            return [];

        var tags = new List<string>
        {
            "source-generated"
        };

        if (!string.IsNullOrWhiteSpace(source.Language))
            tags.Add(source.Language);

        if (!string.IsNullOrWhiteSpace(source.MediaType))
            tags.Add(source.MediaType.Split('/')[0]);

        if (!string.IsNullOrWhiteSpace(source.Label))
            tags.Add(source.Label);

        return tags;
    }

    private static string ResolveSourceLabel(Source source)
    {
        if (!string.IsNullOrWhiteSpace(source.Label))
            return Trim(source.Label, 80);

        if (!string.IsNullOrWhiteSpace(source.ExternalId))
            return Trim(source.ExternalId, 80);

        if (!string.IsNullOrWhiteSpace(source.Locator))
            return Trim(source.Locator, 80);

        return "selected source";
    }

    private static string Trim(string value, int maxLength)
    {
        var trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength].Trim();
    }
}
