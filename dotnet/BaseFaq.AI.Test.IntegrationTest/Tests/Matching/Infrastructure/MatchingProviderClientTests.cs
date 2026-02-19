using BaseFaq.AI.Business.Common.Models;
using BaseFaq.AI.Business.Common.Providers.Abstractions;
using BaseFaq.AI.Business.Common.Providers.Models;
using BaseFaq.AI.Business.Matching.Service;
using BaseFaq.Models.Tenant.Enums;
using Xunit;

namespace BaseFaq.AI.Test.IntegrationTest.Tests.Matching.Infrastructure;

public sealed class MatchingProviderClientTests
{
    [Fact]
    public async Task RankAsync_UsesEmbeddings_WhenEmbeddingsAreAvailable()
    {
        var providerContext = new AiProviderContext("openai", "text-embedding-3-large", null, "secret");
        var runtimeContext =
            BuildRuntimeContext(providerContext, AiProviderStyle.OpenAiCompatible, "https://api.openai.com/v1");
        var candidate1Id = Guid.NewGuid();
        var candidate2Id = Guid.NewGuid();

        var resolver = new FakeRuntimeContextResolver(runtimeContext);
        var embeddings = new FakeEmbeddingsGateway(
            new AiEmbeddingsResult(
            [
                [1f, 0f],
                [1f, 0f],
                [0f, 1f]
            ]));
        var completion = new FakeTextCompletionGateway(new AiTextCompletionResult("{\"items\":[]}"));
        var client = new MatchingProviderClient(resolver, embeddings, completion);

        var ranked = await client.RankAsync(
            providerContext,
            "query",
            [(candidate1Id, "candidate one"), (candidate2Id, "candidate two")],
            5,
            CancellationToken.None);

        Assert.Equal(1, embeddings.CallCount);
        Assert.Equal(0, completion.CallCount);
        Assert.Single(ranked);
        Assert.Equal(candidate1Id, ranked[0].FaqItemId);
    }

    [Fact]
    public async Task RankAsync_FallsBackToCompletion_WhenEmbeddingsAreNotSupported()
    {
        var providerContext = new AiProviderContext("anthropic", "claude-3-5-sonnet", null, "secret");
        var runtimeContext =
            BuildRuntimeContext(providerContext, AiProviderStyle.Anthropic, "https://api.anthropic.com/v1");
        var candidate1Id = Guid.NewGuid();
        var candidate2Id = Guid.NewGuid();

        var resolver = new FakeRuntimeContextResolver(runtimeContext);
        var embeddings = new ThrowingEmbeddingsGateway(new NotSupportedException("Embeddings not supported."));
        var completion = new FakeTextCompletionGateway(
            new AiTextCompletionResult(
                $"```json\n{{\"items\":[{{\"id\":\"{candidate2Id:D}\",\"score\":0.9}},{{\"id\":\"{candidate1Id:D}\",\"score\":0.6}}]}}\n```"));
        var client = new MatchingProviderClient(resolver, embeddings, completion);

        var ranked = await client.RankAsync(
            providerContext,
            "query",
            [(candidate1Id, "candidate one"), (candidate2Id, "candidate two")],
            5,
            CancellationToken.None);

        Assert.Equal(1, embeddings.CallCount);
        Assert.Equal(1, completion.CallCount);
        Assert.Equal(2, ranked.Length);
        Assert.Equal(candidate2Id, ranked[0].FaqItemId);
        Assert.Equal(candidate1Id, ranked[1].FaqItemId);
    }

    private static AiProviderRuntimeContext BuildRuntimeContext(
        AiProviderContext providerContext,
        AiProviderStyle style,
        string baseUrl)
    {
        return new AiProviderRuntimeContext(
            providerContext,
            new AiProviderProfile(providerContext.Provider, style, baseUrl, true, true),
            new Uri(baseUrl),
            providerContext.ApiKey ?? string.Empty,
            null,
            null);
    }

    private sealed class FakeRuntimeContextResolver(AiProviderRuntimeContext runtimeContext)
        : IAiProviderRuntimeContextResolver
    {
        public AiProviderRuntimeContext Resolve(AiProviderContext providerContext, AiCommandType commandType)
        {
            return runtimeContext;
        }
    }

    private sealed class FakeEmbeddingsGateway(AiEmbeddingsResult result) : IAiEmbeddingsGateway
    {
        public int CallCount { get; private set; }

        public Task<AiEmbeddingsResult> CreateEmbeddingsAsync(
            AiProviderRuntimeContext runtimeContext,
            AiEmbeddingsRequest request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            return Task.FromResult(result);
        }
    }

    private sealed class ThrowingEmbeddingsGateway(Exception exception) : IAiEmbeddingsGateway
    {
        public int CallCount { get; private set; }

        public Task<AiEmbeddingsResult> CreateEmbeddingsAsync(
            AiProviderRuntimeContext runtimeContext,
            AiEmbeddingsRequest request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            return Task.FromException<AiEmbeddingsResult>(exception);
        }
    }

    private sealed class FakeTextCompletionGateway(AiTextCompletionResult result) : IAiTextCompletionGateway
    {
        public int CallCount { get; private set; }

        public Task<AiTextCompletionResult> CompleteAsync(
            AiProviderRuntimeContext runtimeContext,
            AiTextCompletionRequest request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            return Task.FromResult(result);
        }
    }
}