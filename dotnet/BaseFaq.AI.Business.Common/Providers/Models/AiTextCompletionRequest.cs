namespace BaseFaq.AI.Business.Common.Providers.Models;

public sealed record AiTextCompletionRequest(
    string SystemPrompt,
    string UserPrompt,
    double Temperature = 0.1);