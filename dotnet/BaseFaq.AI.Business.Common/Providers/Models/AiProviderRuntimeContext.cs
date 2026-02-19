using BaseFaq.AI.Business.Common.Models;

namespace BaseFaq.AI.Business.Common.Providers.Models;

public sealed record AiProviderRuntimeContext(
    AiProviderContext ProviderContext,
    AiProviderProfile Profile,
    Uri BaseUri,
    string ApiKey,
    string? Deployment,
    string? ApiVersion);