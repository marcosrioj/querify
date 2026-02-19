namespace BaseFaq.AI.Business.Common.Providers.Models;

public sealed record AiProviderProfile(
    string Name,
    AiProviderStyle Style,
    string? DefaultBaseUrl,
    bool SupportsGeneration,
    bool SupportsMatching,
    string? UnsupportedReason = null);