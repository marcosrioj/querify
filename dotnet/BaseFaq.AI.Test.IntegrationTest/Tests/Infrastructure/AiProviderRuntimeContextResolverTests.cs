using BaseFaq.AI.Business.Common.Models;
using BaseFaq.AI.Business.Common.Providers.Infrastructure;
using BaseFaq.AI.Business.Common.Providers.Models;
using BaseFaq.Models.Tenant.Enums;
using Xunit;

namespace BaseFaq.AI.Test.IntegrationTest.Tests.Infrastructure;

public sealed class AiProviderRuntimeContextResolverTests
{
    [Fact]
    public void Resolve_Supports_AllSeededProviderNames()
    {
        var registry = new AiProviderProfileRegistry();

        var providerNames = new[]
        {
            AiProviderNames.OpenAi,
            AiProviderNames.Anthropic,
            AiProviderNames.Google,
            AiProviderNames.AzureOpenAi,
            AiProviderNames.AwsBedrock,
            AiProviderNames.Cohere,
            AiProviderNames.Mistral,
            AiProviderNames.TogetherAi,
            AiProviderNames.FireworksAi,
            AiProviderNames.Groq,
            AiProviderNames.VoyageAi,
            AiProviderNames.JinaAi
        };

        foreach (var providerName in providerNames)
        {
            var profile = registry.Resolve(providerName);
            Assert.Equal(providerName, profile.Name);
        }
    }

    [Fact]
    public void Resolve_ParsesEndpointAwareCredential_ForAzureOpenAi()
    {
        var resolver = new AiProviderRuntimeContextResolver(new AiProviderProfileRegistry());

        var runtime = resolver.Resolve(
            new AiProviderContext(
                AiProviderNames.AzureOpenAi,
                "gpt-5.2",
                null,
                "https://contoso.openai.azure.com|secret-key|my-deployment|2024-10-21"),
            AiCommandType.Generation);

        Assert.Equal("contoso.openai.azure.com", runtime.BaseUri.Host);
        Assert.Equal("secret-key", runtime.ApiKey);
        Assert.Equal("my-deployment", runtime.Deployment);
        Assert.Equal("2024-10-21", runtime.ApiVersion);
    }

    [Fact]
    public void Resolve_Throws_WhenProviderDoesNotSupportCommand()
    {
        var resolver = new AiProviderRuntimeContextResolver(new AiProviderProfileRegistry());

        Assert.Throws<NotSupportedException>(() =>
            resolver.Resolve(
                new AiProviderContext(
                    AiProviderNames.VoyageAi,
                    "voyage-3.5",
                    null,
                    "voyage-secret"),
                AiCommandType.Generation));
    }
}