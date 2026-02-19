using BaseFaq.AI.Business.Common.Abstractions;
using BaseFaq.AI.Business.Common.Infrastructure;
using BaseFaq.AI.Business.Common.Providers.Abstractions;
using BaseFaq.AI.Business.Common.Providers.Infrastructure;
using BaseFaq.AI.Business.Common.Providers.Strategies.Anthropic;
using BaseFaq.AI.Business.Common.Providers.Strategies.AzureOpenAi;
using BaseFaq.AI.Business.Common.Providers.Strategies.Cohere;
using BaseFaq.AI.Business.Common.Providers.Strategies.Google;
using BaseFaq.AI.Business.Common.Providers.Strategies.OpenAiCompatible;
using BaseFaq.AI.Business.Common.Providers.Strategies.Voyage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BaseFaq.AI.Business.Common.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAiBusinessCommon(this IServiceCollection services)
    {
        services.TryAddScoped<IAiProviderContextResolver, AiProviderContextResolver>();
        services.TryAddScoped<IFaqDbContextFactory, FaqDbContextFactory>();

        services.TryAddScoped<IAiProviderProfileRegistry, AiProviderProfileRegistry>();
        services.TryAddScoped<IAiProviderRuntimeContextResolver, AiProviderRuntimeContextResolver>();
        services.TryAddScoped<ProviderHttpJsonClient>();

        services.TryAddScoped<IAiTextCompletionGateway, AiTextCompletionGateway>();
        services.TryAddScoped<IAiEmbeddingsGateway, AiEmbeddingsGateway>();

        services.TryAddEnumerable(
            ServiceDescriptor.Scoped<IAiTextCompletionStrategy, OpenAiCompatibleTextCompletionStrategy>());
        services.TryAddEnumerable(
            ServiceDescriptor.Scoped<IAiTextCompletionStrategy, AnthropicTextCompletionStrategy>());
        services.TryAddEnumerable(
            ServiceDescriptor.Scoped<IAiTextCompletionStrategy, GoogleTextCompletionStrategy>());
        services.TryAddEnumerable(
            ServiceDescriptor.Scoped<IAiTextCompletionStrategy, CohereTextCompletionStrategy>());
        services.TryAddEnumerable(
            ServiceDescriptor.Scoped<IAiTextCompletionStrategy, AzureOpenAiTextCompletionStrategy>());

        services.TryAddEnumerable(
            ServiceDescriptor.Scoped<IAiEmbeddingsStrategy, OpenAiCompatibleEmbeddingsStrategy>());
        services.TryAddEnumerable(
            ServiceDescriptor.Scoped<IAiEmbeddingsStrategy, GoogleEmbeddingsStrategy>());
        services.TryAddEnumerable(
            ServiceDescriptor.Scoped<IAiEmbeddingsStrategy, CohereEmbeddingsStrategy>());
        services.TryAddEnumerable(
            ServiceDescriptor.Scoped<IAiEmbeddingsStrategy, AzureOpenAiEmbeddingsStrategy>());
        services.TryAddEnumerable(
            ServiceDescriptor.Scoped<IAiEmbeddingsStrategy, VoyageEmbeddingsStrategy>());
        return services;
    }
}