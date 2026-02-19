namespace BaseFaq.AI.Business.Common.Providers.Models;

public sealed record AiEmbeddingsRequest(IReadOnlyList<string> Inputs);