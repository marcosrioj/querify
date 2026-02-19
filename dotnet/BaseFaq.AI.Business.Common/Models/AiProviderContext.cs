namespace BaseFaq.AI.Business.Common.Models;

public sealed record AiProviderContext(
    string Provider,
    string Model,
    string? Prompt,
    string? ApiKey);