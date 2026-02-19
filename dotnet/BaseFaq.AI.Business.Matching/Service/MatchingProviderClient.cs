using System.Text;
using System.Text.Json;
using System.Globalization;
using BaseFaq.AI.Business.Common.Models;
using BaseFaq.AI.Business.Common.Utilities;
using BaseFaq.AI.Business.Common.Providers.Abstractions;
using BaseFaq.AI.Business.Common.Providers.Models;
using BaseFaq.AI.Business.Matching.Abstractions;
using BaseFaq.Models.Ai.Contracts.Matching;
using BaseFaq.Models.Tenant.Enums;

namespace BaseFaq.AI.Business.Matching.Service;

public sealed class MatchingProviderClient(
    IAiProviderRuntimeContextResolver runtimeContextResolver,
    IAiEmbeddingsGateway embeddingsGateway,
    IAiTextCompletionGateway textCompletionGateway)
    : IMatchingProviderClient
{
    public async Task<MatchingCandidate[]> RankAsync(
        AiProviderContext providerContext,
        string queryText,
        IReadOnlyList<(Guid Id, string Question)> candidates,
        int maxCandidates,
        CancellationToken cancellationToken)
    {
        if (candidates.Count == 0)
        {
            return [];
        }

        var runtimeContext = runtimeContextResolver.Resolve(providerContext, AiCommandType.Matching);
        var candidatesById = candidates.ToDictionary(x => x.Id, x => x.Question);

        var input = new List<string>(candidates.Count + 1) { queryText };
        input.AddRange(candidates.Select(x => x.Question));

        try
        {
            var embeddings = await embeddingsGateway.CreateEmbeddingsAsync(
                runtimeContext,
                new AiEmbeddingsRequest(input),
                cancellationToken);

            if (embeddings.Vectors.Count != input.Count)
            {
                throw new InvalidOperationException("Provider returned an invalid embeddings payload.");
            }

            var queryVector = embeddings.Vectors[0];

            return candidates
                .Select((candidate, index) => new MatchingCandidate(
                    candidate.Id,
                    Math.Round(CosineSimilarity(queryVector, embeddings.Vectors[index + 1]), 4)))
                .Where(x => x.SimilarityScore > 0)
                .OrderByDescending(x => x.SimilarityScore)
                .ThenBy(x => x.FaqItemId)
                .Take(Math.Max(1, maxCandidates))
                .ToArray();
        }
        catch (NotSupportedException)
        {
            return await RankByCompletionFallbackAsync(
                runtimeContext,
                queryText,
                candidatesById,
                maxCandidates,
                cancellationToken);
        }
        catch (InvalidOperationException ex) when (LooksLikeEmbeddingsEndpointUnavailable(ex))
        {
            return await RankByCompletionFallbackAsync(
                runtimeContext,
                queryText,
                candidatesById,
                maxCandidates,
                cancellationToken);
        }
    }

    private async Task<MatchingCandidate[]> RankByCompletionFallbackAsync(
        AiProviderRuntimeContext runtimeContext,
        string queryText,
        IReadOnlyDictionary<Guid, string> candidatesById,
        int maxCandidates,
        CancellationToken cancellationToken)
    {
        var request = new AiTextCompletionRequest(
            BuildFallbackSystemPrompt(),
            BuildFallbackUserPrompt(queryText, candidatesById, maxCandidates),
            Temperature: 0);

        var completion = await textCompletionGateway.CompleteAsync(runtimeContext, request, cancellationToken);
        var ranked = ParseCompletionRanking(completion.Content, candidatesById.Keys, maxCandidates);

        if (ranked.Length == 0)
        {
            throw new InvalidOperationException(
                "Fallback matching completion returned no valid candidates.");
        }

        return ranked;
    }

    private static bool LooksLikeEmbeddingsEndpointUnavailable(InvalidOperationException ex)
    {
        var message = ex.Message;
        return message.Contains("status 404", StringComparison.OrdinalIgnoreCase) ||
               message.Contains("status 405", StringComparison.OrdinalIgnoreCase) ||
               message.Contains("status 501", StringComparison.OrdinalIgnoreCase) ||
               (message.Contains("embeddings", StringComparison.OrdinalIgnoreCase) &&
                message.Contains("not found", StringComparison.OrdinalIgnoreCase));
    }

    private static string BuildFallbackSystemPrompt()
    {
        return
            "You are a semantic FAQ matcher. Return JSON only with schema {\"items\":[{\"id\":\"guid\",\"score\":0..1}]}. " +
            "Use only provided candidate ids. Sort by score descending.";
    }

    private static string BuildFallbackUserPrompt(
        string queryText,
        IReadOnlyDictionary<Guid, string> candidatesById,
        int maxCandidates)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Query:");
        builder.AppendLine(queryText);
        builder.AppendLine();
        builder.AppendLine($"Return at most {Math.Max(1, maxCandidates)} candidates.");
        builder.AppendLine("Candidates:");

        foreach (var candidate in candidatesById.OrderBy(x => x.Key))
        {
            builder.AppendLine($"- id={candidate.Key:D}; question={candidate.Value}");
        }

        return builder.ToString();
    }

    private static MatchingCandidate[] ParseCompletionRanking(
        string content,
        IEnumerable<Guid> validIds,
        int maxCandidates)
    {
        var raw = JsonPayloadReader.ExtractJsonObject(content);
        using var document = JsonDocument.Parse(raw);
        var root = document.RootElement;
        var items = ResolveItems(root);

        if (items.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException("Matching completion payload must contain an items array.");
        }

        var validIdSet = validIds.ToHashSet();
        var resultById = new Dictionary<Guid, double>();

        foreach (var item in items.EnumerateArray())
        {
            if (!TryReadGuid(item, "id", out var id) &&
                !TryReadGuid(item, "faqItemId", out id))
            {
                continue;
            }

            if (!validIdSet.Contains(id) || !TryReadScore(item, out var score))
            {
                continue;
            }

            if (!resultById.TryGetValue(id, out var existingScore) || score > existingScore)
            {
                resultById[id] = score;
            }
        }

        return resultById
            .Select(x => new MatchingCandidate(x.Key, Math.Round(x.Value, 4)))
            .Where(x => x.SimilarityScore > 0)
            .OrderByDescending(x => x.SimilarityScore)
            .ThenBy(x => x.FaqItemId)
            .Take(Math.Max(1, maxCandidates))
            .ToArray();
    }

    private static JsonElement ResolveItems(JsonElement root)
    {
        if (root.ValueKind == JsonValueKind.Array)
        {
            return root;
        }

        if (root.ValueKind == JsonValueKind.Object &&
            TryGetPropertyIgnoreCase(root, "items", out var items))
        {
            return items;
        }

        return default;
    }

    private static bool TryReadGuid(JsonElement item, string propertyName, out Guid id)
    {
        id = Guid.Empty;
        if (item.ValueKind != JsonValueKind.Object ||
            !TryGetPropertyIgnoreCase(item, propertyName, out var idElement) ||
            idElement.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        return Guid.TryParse(idElement.GetString(), out id);
    }

    private static bool TryReadScore(JsonElement item, out double score)
    {
        score = 0;
        if (item.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        if (!TryGetPropertyIgnoreCase(item, "score", out var scoreElement) &&
            !TryGetPropertyIgnoreCase(item, "similarity", out scoreElement) &&
            !TryGetPropertyIgnoreCase(item, "similarityScore", out scoreElement))
        {
            return false;
        }

        if (scoreElement.ValueKind == JsonValueKind.Number &&
            scoreElement.TryGetDouble(out var number))
        {
            score = Math.Clamp(number, 0, 1);
            return true;
        }

        if (scoreElement.ValueKind == JsonValueKind.String &&
            double.TryParse(
                scoreElement.GetString(),
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out var parsed))
        {
            score = Math.Clamp(parsed, 0, 1);
            return true;
        }

        return false;
    }

    private static bool TryGetPropertyIgnoreCase(JsonElement element, string propertyName, out JsonElement value)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (property.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase))
                {
                    value = property.Value;
                    return true;
                }
            }
        }

        value = default;
        return false;
    }

    private static double CosineSimilarity(float[] left, float[] right)
    {
        if (left.Length == 0 || right.Length == 0 || left.Length != right.Length)
        {
            return 0;
        }

        double dot = 0;
        double normLeft = 0;
        double normRight = 0;

        for (var i = 0; i < left.Length; i++)
        {
            dot += left[i] * right[i];
            normLeft += left[i] * left[i];
            normRight += right[i] * right[i];
        }

        if (normLeft <= 0 || normRight <= 0)
        {
            return 0;
        }

        return dot / (Math.Sqrt(normLeft) * Math.Sqrt(normRight));
    }
}