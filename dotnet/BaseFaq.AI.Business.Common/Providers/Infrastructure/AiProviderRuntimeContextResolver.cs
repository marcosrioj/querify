using BaseFaq.AI.Business.Common.Models;
using BaseFaq.AI.Business.Common.Providers.Abstractions;
using BaseFaq.AI.Business.Common.Providers.Models;
using BaseFaq.Models.Tenant.Enums;

namespace BaseFaq.AI.Business.Common.Providers.Infrastructure;

public sealed class AiProviderRuntimeContextResolver(IAiProviderProfileRegistry providerProfileRegistry)
    : IAiProviderRuntimeContextResolver
{
    private const string AzureDefaultApiVersion = "2024-10-21";

    public AiProviderRuntimeContext Resolve(AiProviderContext providerContext, AiCommandType commandType)
    {
        ArgumentNullException.ThrowIfNull(providerContext);

        var profile = providerProfileRegistry.Resolve(providerContext.Provider);
        EnsureCommandSupport(profile, commandType, providerContext.Model);

        var credential = ProviderCredentialParser.Parse(providerContext.ApiKey);
        var baseUri = ResolveBaseUri(profile, credential);

        var deployment = string.IsNullOrWhiteSpace(credential.DeploymentOverride)
            ? providerContext.Model
            : credential.DeploymentOverride;

        var apiVersion = string.IsNullOrWhiteSpace(credential.ApiVersionOverride)
            ? (profile.Style == AiProviderStyle.AzureOpenAi ? AzureDefaultApiVersion : null)
            : credential.ApiVersionOverride;

        if (profile.Style == AiProviderStyle.AzureOpenAi && string.IsNullOrWhiteSpace(deployment))
        {
            throw new InvalidOperationException(
                "Azure OpenAI requires a deployment name. Use model as deployment or provide credential format with deployment override.");
        }

        return new AiProviderRuntimeContext(
            providerContext,
            profile,
            baseUri,
            credential.ApiKey,
            deployment,
            apiVersion);
    }

    private static Uri ResolveBaseUri(AiProviderProfile profile, AiProviderCredential credential)
    {
        if (credential.EndpointOverride is not null)
        {
            return credential.EndpointOverride;
        }

        if (!string.IsNullOrWhiteSpace(profile.DefaultBaseUrl) &&
            Uri.TryCreate(profile.DefaultBaseUrl, UriKind.Absolute, out var defaultUri))
        {
            return defaultUri;
        }

        var reason = string.IsNullOrWhiteSpace(profile.UnsupportedReason)
            ? "No base URL configured."
            : profile.UnsupportedReason;

        throw new NotSupportedException($"Provider '{profile.Name}' has no resolvable endpoint. {reason}");
    }

    private static void EnsureCommandSupport(AiProviderProfile profile, AiCommandType commandType, string model)
    {
        var normalizedModel = string.IsNullOrWhiteSpace(model) ? string.Empty : model.Trim();
        if (normalizedModel.Length == 0)
        {
            throw new InvalidOperationException(
                $"Provider '{profile.Name}' has no model configured for command '{commandType}'.");
        }

        if (normalizedModel.StartsWith("external-", StringComparison.OrdinalIgnoreCase))
        {
            throw new NotSupportedException(
                $"Provider '{profile.Name}' model '{model}' requires external orchestration and is not executable by this worker runtime.");
        }

        var supportsCommand = commandType switch
        {
            AiCommandType.Generation => profile.SupportsGeneration,
            AiCommandType.Matching => profile.SupportsMatching,
            _ => false
        };

        if (supportsCommand)
        {
            return;
        }

        var reason = string.IsNullOrWhiteSpace(profile.UnsupportedReason)
            ? "No runtime strategy is available for this command/provider combination."
            : profile.UnsupportedReason;

        throw new NotSupportedException(
            $"Provider '{profile.Name}' is not supported for command '{commandType}'. {reason}");
    }
}