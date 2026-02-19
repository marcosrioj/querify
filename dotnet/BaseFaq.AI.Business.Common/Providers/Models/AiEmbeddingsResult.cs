namespace BaseFaq.AI.Business.Common.Providers.Models;

public sealed record AiEmbeddingsResult(IReadOnlyList<float[]> Vectors);