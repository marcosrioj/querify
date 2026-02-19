namespace BaseFaq.AI.Business.Common.Providers.Models;

public sealed record AiProviderCredential(
    string ApiKey,
    Uri? EndpointOverride,
    string? DeploymentOverride,
    string? ApiVersionOverride);