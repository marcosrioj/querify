using BaseFaq.AI.Business.Common.Infrastructure;
using BaseFaq.AI.Business.Common.Models;
using BaseFaq.AI.Business.Common.Abstractions;
using BaseFaq.AI.Business.Common.Providers.Infrastructure;
using BaseFaq.AI.Business.Common.Providers.Models;
using BaseFaq.AI.Business.Common.Providers.Strategies.OpenAiCompatible;
using BaseFaq.AI.Business.Generation.Service;
using BaseFaq.AI.Business.Matching.Service;
using BaseFaq.AI.Test.IntegrationTest.Helpers.Infrastructure;
using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Common.EntityFramework.Tenant.Entities;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Faq.Common.Persistence.FaqDb;
using BaseFaq.Faq.Common.Persistence.FaqDb.Entities;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.Faq.Enums;
using BaseFaq.Models.Ai.Contracts.Generation;
using BaseFaq.Models.Ai.Contracts.Matching;
using BaseFaq.Models.Tenant.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;
using FaqEntity = BaseFaq.Faq.Common.Persistence.FaqDb.Entities.Faq;

namespace BaseFaq.AI.Test.IntegrationTest.Tests.GenerationMatching;

public sealed class OpenAiGenerationMatchingFlowTests
{
    [Fact]
    public async Task Live_OpenAi_Generates_FaqItem_And_Matches_Similar_Candidates()
    {
        if (!OpenAiLiveTestSettings.TryLoad(out var settings))
        {
            return;
        }

        using var tenantDatabase = TestDatabase.Create("bf_ai_tenant_test");
        using var faqDatabase = TestDatabase.Create("bf_ai_faq_test");

        var runtimeState = CreateRuntimeState();

        await using var tenantDbContext = await CreateAndSeedTenantDbContextAsync(
            tenantDatabase.ConnectionString,
            runtimeState.TenantId,
            runtimeState.RequestedByUserId,
            runtimeState.Configuration,
            faqDatabase.ConnectionString,
            settings);

        var faqScenario = await CreateAndSeedFaqScenarioAsync(
            faqDatabase.ConnectionString,
            runtimeState.TenantId,
            runtimeState.RequestedByUserId,
            runtimeState.Configuration);

        var services = BuildLiveFlowServices(
            tenantDbContext,
            runtimeState.TenantId,
            faqDatabase.ConnectionString,
            runtimeState.Configuration,
            settings.ApiKey);

        var generationRequested = CreateGenerationRequest(
            faqScenario.FaqId,
            runtimeState.TenantId,
            runtimeState.RequestedByUserId);

        var generatedFaqItemId = await GenerateAndPersistFaqItemAsync(
            services,
            generationRequested,
            runtimeState.TenantId);

        var matchingRequested = CreateMatchingRequest(
            generatedFaqItemId,
            runtimeState.TenantId,
            runtimeState.RequestedByUserId,
            settings.SimilarCandidateQuestion);

        var rankedCandidates = await services.MatchingExecutionService.ExecuteAsync(
            matchingRequested,
            CancellationToken.None);

        AssertExpectedMatchingRanking(rankedCandidates, faqScenario.SimilarCandidateFaqItemId);
    }

    private static RuntimeState CreateRuntimeState()
    {
        var tenantId = Guid.NewGuid();
        var requestedByUserId = Guid.NewGuid();
        var aiUserId = Guid.NewGuid();
        var configuration = BuildConfiguration(aiUserId);

        return new RuntimeState(tenantId, requestedByUserId, configuration);
    }

    private static async Task<TenantDbContext> CreateAndSeedTenantDbContextAsync(
        string tenantConnectionString,
        Guid tenantId,
        Guid requestedByUserId,
        IConfiguration configuration,
        string faqConnectionString,
        OpenAiLiveTestSettings settings)
    {
        var tenantDbContext = CreateTenantDbContext(
            tenantConnectionString,
            tenantId,
            requestedByUserId,
            configuration);

        await tenantDbContext.Database.MigrateAsync();
        await SeedTenantProviderScenarioAsync(tenantDbContext, tenantId, faqConnectionString, settings);
        return tenantDbContext;
    }

    private static async Task<(Guid FaqId, Guid SimilarCandidateFaqItemId)> CreateAndSeedFaqScenarioAsync(
        string faqConnectionString,
        Guid tenantId,
        Guid requestedByUserId,
        IConfiguration configuration)
    {
        await using var faqSeedDbContext = CreateFaqDbContext(
            faqConnectionString,
            tenantId,
            requestedByUserId,
            configuration);
        await faqSeedDbContext.Database.MigrateAsync();
        return await SeedFaqScenarioAsync(faqSeedDbContext, tenantId);
    }

    private static LiveFlowServices BuildLiveFlowServices(
        TenantDbContext tenantDbContext,
        Guid tenantId,
        string faqConnectionString,
        IConfiguration configuration,
        string runtimeApiKey)
    {
        var aiProviderContextResolver = new OpenAiRuntimeApiKeyContextResolver(
            new AiProviderContextResolver(tenantDbContext),
            runtimeApiKey);
        var faqConnectionStringProvider = new SingleTenantConnectionStringProvider(tenantId, faqConnectionString);
        var faqDbContextFactory = new FaqDbContextFactory(faqConnectionStringProvider, configuration);

        var runtimeContextResolver = new AiProviderRuntimeContextResolver(new AiProviderProfileRegistry());
        var providerHttpClient = new ProviderHttpJsonClient();
        var textCompletionGateway = new AiTextCompletionGateway(
        [
            new OpenAiCompatibleTextCompletionStrategy(providerHttpClient)
        ]);
        var embeddingsGateway = new AiEmbeddingsGateway(
        [
            new OpenAiCompatibleEmbeddingsStrategy(providerHttpClient)
        ]);

        var generationProviderClient = new GenerationProviderClient(runtimeContextResolver, textCompletionGateway);
        var matchingProviderClient =
            new MatchingProviderClient(runtimeContextResolver, embeddingsGateway, textCompletionGateway);
        var matchingExecutionService =
            new MatchingExecutionService(aiProviderContextResolver, faqDbContextFactory, matchingProviderClient);

        return new LiveFlowServices(
            aiProviderContextResolver,
            faqDbContextFactory,
            new ContentRefStudyService(),
            new GenerationPromptBuilder(),
            generationProviderClient,
            matchingExecutionService);
    }

    private static FaqGenerationRequestedV1 CreateGenerationRequest(
        Guid faqId,
        Guid tenantId,
        Guid requestedByUserId)
    {
        return new FaqGenerationRequestedV1
        {
            CorrelationId = Guid.NewGuid(),
            FaqId = faqId,
            TenantId = tenantId,
            RequestedByUserId = requestedByUserId,
            Language = "pt-BR",
            IdempotencyKey = Guid.NewGuid().ToString("N"),
            RequestedUtc = DateTime.UtcNow
        };
    }

    private static FaqMatchingRequestedV1 CreateMatchingRequest(
        Guid faqItemId,
        Guid tenantId,
        Guid requestedByUserId,
        string query)
    {
        return new FaqMatchingRequestedV1
        {
            CorrelationId = Guid.NewGuid(),
            TenantId = tenantId,
            FaqItemId = faqItemId,
            RequestedByUserId = requestedByUserId,
            Query = query,
            Language = "pt-BR",
            IdempotencyKey = Guid.NewGuid().ToString("N"),
            RequestedUtc = DateTime.UtcNow
        };
    }

    private static async Task<Guid> GenerateAndPersistFaqItemAsync(
        LiveFlowServices services,
        FaqGenerationRequestedV1 generationRequested,
        Guid tenantId)
    {
        await using var generationDbContext = services.FaqDbContextFactory.Create(tenantId);
        var contentRefs = await LoadGenerationContentRefsAsync(generationDbContext, generationRequested.FaqId, tenantId);
        var studiedRefs = services.ContentRefStudyService.Study(contentRefs);

        var generationProviderContext = await services.AiProviderContextResolver.ResolveAsync(
            tenantId,
            AiCommandType.Generation);
        var promptData = services.GenerationPromptBuilder.BuildPromptData(
            generationRequested,
            studiedRefs,
            generationProviderContext);
        var generatedDraft = await services.GenerationProviderClient.GenerateDraftAsync(
            generationProviderContext,
            promptData,
            CancellationToken.None);

        AssertGeneratedDraftIsComplete(generatedDraft.Question, generatedDraft.Summary, generatedDraft.Answer);

        var generatedFaqItem = CreateGeneratedFaqItemEntity(
            generationRequested.FaqId,
            tenantId,
            generatedDraft.Question,
            generatedDraft.Summary,
            generatedDraft.Answer,
            generatedDraft.Confidence);

        generationDbContext.FaqItems.Add(generatedFaqItem);
        await generationDbContext.SaveChangesAsync();
        return generatedFaqItem.Id;
    }

    private static Task<List<(ContentRefKind Kind, string Locator)>> LoadGenerationContentRefsAsync(
        FaqDbContext faqDbContext,
        Guid faqId,
        Guid tenantId)
    {
        return faqDbContext.FaqContentRefs
            .AsNoTracking()
            .Where(x => x.FaqId == faqId && x.TenantId == tenantId)
            .Select(x => new ValueTuple<ContentRefKind, string>(x.ContentRef.Kind, x.ContentRef.Locator))
            .ToListAsync();
    }

    private static FaqItem CreateGeneratedFaqItemEntity(
        Guid faqId,
        Guid tenantId,
        string question,
        string summary,
        string answer,
        int confidence)
    {
        return new FaqItem
        {
            Question = Truncate(question, FaqItem.MaxQuestionLength),
            ShortAnswer = Truncate(summary, FaqItem.MaxShortAnswerLength),
            Answer = Truncate(answer, FaqItem.MaxAnswerLength),
            Sort = 99,
            VoteScore = 0,
            AiConfidenceScore = Math.Clamp(confidence, 0, 100),
            IsActive = true,
            FaqId = faqId,
            TenantId = tenantId
        };
    }

    private static void AssertGeneratedDraftIsComplete(string question, string summary, string answer)
    {
        Assert.False(string.IsNullOrWhiteSpace(question));
        Assert.False(string.IsNullOrWhiteSpace(summary));
        Assert.False(string.IsNullOrWhiteSpace(answer));
    }

    private static void AssertExpectedMatchingRanking(
        MatchingCandidate[] rankedCandidates,
        Guid expectedTopCandidateFaqItemId)
    {
        Assert.NotEmpty(rankedCandidates);
        Assert.Equal(expectedTopCandidateFaqItemId, rankedCandidates[0].FaqItemId);
        Assert.True(rankedCandidates[0].SimilarityScore > 0);
    }

    private static IConfiguration BuildConfiguration(Guid aiUserId)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Ai:UserId"] = aiUserId.ToString("D")
            })
            .Build();
    }

    private static TenantDbContext CreateTenantDbContext(
        string connectionString,
        Guid tenantId,
        Guid userId,
        IConfiguration configuration)
    {
        var options = new DbContextOptionsBuilder<TenantDbContext>()
            .UseNpgsql(connectionString)
            .EnableDetailedErrors()
            .EnableSensitiveDataLogging()
            .Options;

        return new TenantDbContext(
            options,
            new TestSessionService(tenantId, userId),
            configuration,
            new StaticTenantConnectionStringProvider(connectionString),
            new HttpContextAccessor());
    }

    private static FaqDbContext CreateFaqDbContext(
        string connectionString,
        Guid tenantId,
        Guid userId,
        IConfiguration configuration)
    {
        var options = new DbContextOptionsBuilder<FaqDbContext>()
            .UseNpgsql(connectionString)
            .EnableDetailedErrors()
            .EnableSensitiveDataLogging()
            .Options;

        return new FaqDbContext(
            options,
            new TestSessionService(tenantId, userId),
            configuration,
            new StaticTenantConnectionStringProvider(connectionString),
            new HttpContextAccessor());
    }

    private static async Task SeedTenantProviderScenarioAsync(
        TenantDbContext tenantDbContext,
        Guid tenantId,
        string faqConnectionString,
        OpenAiLiveTestSettings settings)
    {
        var tenant = new Tenant
        {
            Id = tenantId,
            Slug = $"tenant-{tenantId:N}",
            Name = "AI Integration Test Tenant",
            Edition = TenantEdition.Free,
            App = AppEnum.Faq,
            ConnectionString = faqConnectionString,
            IsActive = true,
            UserId = Guid.NewGuid()
        };

        var generationProvider = new AiProvider
        {
            Provider = AiProviderNames.OpenAi,
            Model = settings.GenerationModel,
            Prompt =
                "Gere um FAQ item em portugues (pt-BR), com foco em conta/acesso, retornando JSON valido para question/summary/answer/confidence.",
            Command = AiCommandType.Generation
        };

        var matchingProvider = new AiProvider
        {
            Provider = AiProviderNames.OpenAi,
            Model = settings.MatchingModel,
            Prompt = "Use embeddings para matching semantico entre perguntas de FAQ.",
            Command = AiCommandType.Matching
        };

        tenantDbContext.Tenants.Add(tenant);
        tenantDbContext.AiProviders.AddRange(generationProvider, matchingProvider);
        await tenantDbContext.SaveChangesAsync();

        tenantDbContext.TenantAiProviders.AddRange(
            new TenantAiProvider
            {
                TenantId = tenantId,
                AiProviderId = generationProvider.Id,
                AiProviderKey = settings.TenantDbProviderKey
            },
            new TenantAiProvider
            {
                TenantId = tenantId,
                AiProviderId = matchingProvider.Id,
                AiProviderKey = settings.TenantDbProviderKey
            });

        await tenantDbContext.SaveChangesAsync();
    }

    private static async Task<(Guid FaqId, Guid SimilarCandidateFaqItemId)> SeedFaqScenarioAsync(
        FaqDbContext faqDbContext,
        Guid tenantId)
    {
        var faq = new FaqEntity
        {
            Name = "Central de ajuda",
            Language = "pt-BR",
            Status = FaqStatus.Draft,
            SortStrategy = FaqSortStrategy.Sort,
            CtaEnabled = false,
            CtaTarget = CtaTarget.Self,
            TenantId = tenantId
        };

        faqDbContext.Faqs.Add(faq);
        await faqDbContext.SaveChangesAsync();

        var contentRef = new ContentRef
        {
            Kind = ContentRefKind.Web,
            Locator = "https://docs.basefaq.test/conta/redefinir-senha",
            Label = "Guia de redefinicao de senha",
            Scope = "Conta e acesso",
            TenantId = tenantId
        };

        faqDbContext.ContentRefs.Add(contentRef);
        await faqDbContext.SaveChangesAsync();

        faqDbContext.FaqContentRefs.Add(new FaqContentRef
        {
            FaqId = faq.Id,
            ContentRefId = contentRef.Id,
            TenantId = tenantId
        });

        var similarCandidate = new FaqItem
        {
            Question = OpenAiLiveTestSettings.DefaultSimilarCandidateQuestion,
            ShortAnswer = "Use a opcao de esqueci minha senha.",
            Answer = "Clique em 'Esqueci minha senha' na tela de login e siga o e-mail de recuperacao.",
            Sort = 1,
            VoteScore = 0,
            AiConfidenceScore = 70,
            IsActive = true,
            FaqId = faq.Id,
            TenantId = tenantId
        };

        var unrelatedCandidate = new FaqItem
        {
            Question = "Quais formas de pagamento sao aceitas?",
            ShortAnswer = "Aceitamos cartao e pix.",
            Answer = "Voce pode pagar com cartao de credito, debito e pix.",
            Sort = 2,
            VoteScore = 0,
            AiConfidenceScore = 70,
            IsActive = true,
            FaqId = faq.Id,
            TenantId = tenantId
        };

        faqDbContext.FaqItems.AddRange(similarCandidate, unrelatedCandidate);
        await faqDbContext.SaveChangesAsync();

        return (faq.Id, similarCandidate.Id);
    }

    private static string Truncate(string value, int maxLength)
    {
        var normalized = value.Trim();
        if (normalized.Length <= maxLength)
        {
            return normalized;
        }

        return normalized[..maxLength];
    }

    private sealed record RuntimeState(
        Guid TenantId,
        Guid RequestedByUserId,
        IConfiguration Configuration);

    private sealed record LiveFlowServices(
        IAiProviderContextResolver AiProviderContextResolver,
        FaqDbContextFactory FaqDbContextFactory,
        ContentRefStudyService ContentRefStudyService,
        GenerationPromptBuilder GenerationPromptBuilder,
        GenerationProviderClient GenerationProviderClient,
        MatchingExecutionService MatchingExecutionService);

    private sealed class TestSessionService(Guid tenantId, Guid userId) : ISessionService
    {
        public Guid GetTenantId(AppEnum app)
        {
            return tenantId;
        }

        public Guid GetUserId()
        {
            return userId;
        }
    }

    private sealed class StaticTenantConnectionStringProvider(string connectionString) : ITenantConnectionStringProvider
    {
        public string GetConnectionString(Guid tenantId)
        {
            return connectionString;
        }
    }

    private sealed class SingleTenantConnectionStringProvider(Guid tenantId, string connectionString)
        : ITenantConnectionStringProvider
    {
        public string GetConnectionString(Guid requestTenantId)
        {
            if (requestTenantId != tenantId)
            {
                throw new InvalidOperationException(
                    $"Connection string not configured for tenant '{requestTenantId}'.");
            }

            return connectionString;
        }
    }

    private sealed class OpenAiLiveTestSettings(string apiKey, string generationModel, string matchingModel)
    {
        public const string DefaultSimilarCandidateQuestion = "Como redefinir minha senha da conta?";
        private const string DefaultGenerationModel = "gpt-4o-mini";
        private const string DefaultMatchingModel = "text-embedding-3-small";
        private const string DefaultTenantDbProviderKey = "openai-test-placeholder-key";

        public string ApiKey { get; } = apiKey;
        public string GenerationModel { get; } = generationModel;
        public string MatchingModel { get; } = matchingModel;
        public string TenantDbProviderKey => DefaultTenantDbProviderKey;
        public string SimilarCandidateQuestion => DefaultSimilarCandidateQuestion;

        public static bool TryLoad(out OpenAiLiveTestSettings settings)
        {
            settings = null!;

            var configuration = BuildConfiguration();

            var enabledRaw = ResolveConfigValue(
                configuration,
                "BASEFAQ_RUN_OPENAI_INTEGRATION_TESTS",
                "OpenAiIntegrationTest:Enabled");
            if (!IsEnabled(enabledRaw))
            {
                return false;
            }

            var apiKey = ResolveConfigValue(
                configuration,
                "OPENAI_API_KEY",
                "OpenAiIntegrationTest:ApiKey");
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return false;
            }

            var generationModel = ResolveConfigValue(
                configuration,
                "BASEFAQ_OPENAI_GENERATION_MODEL",
                "OpenAiIntegrationTest:GenerationModel");
            if (string.IsNullOrWhiteSpace(generationModel))
            {
                generationModel = DefaultGenerationModel;
            }

            var matchingModel = ResolveConfigValue(
                configuration,
                "BASEFAQ_OPENAI_MATCHING_MODEL",
                "OpenAiIntegrationTest:MatchingModel");
            if (string.IsNullOrWhiteSpace(matchingModel))
            {
                matchingModel = DefaultMatchingModel;
            }

            settings = new OpenAiLiveTestSettings(apiKey.Trim(), generationModel.Trim(), matchingModel.Trim());
            return true;
        }

        private static IConfiguration BuildConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
        }

        private static string? ResolveConfigValue(IConfiguration configuration, params string[] keys)
        {
            foreach (var key in keys)
            {
                var value = configuration[key];
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }

            return null;
        }

        private static bool IsEnabled(string? rawValue)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return false;
            }

            if (bool.TryParse(rawValue, out var parsed))
            {
                return parsed;
            }

            return rawValue.Trim() switch
            {
                "1" => true,
                "yes" => true,
                "on" => true,
                _ => false
            };
        }
    }

    private sealed class OpenAiRuntimeApiKeyContextResolver(
        IAiProviderContextResolver innerResolver,
        string runtimeApiKey)
        : IAiProviderContextResolver
    {
        public async Task<AiProviderContext> ResolveAsync(
            Guid tenantId,
            AiCommandType commandType,
            CancellationToken cancellationToken = default)
        {
            var context = await innerResolver.ResolveAsync(tenantId, commandType, cancellationToken);
            return context with { ApiKey = runtimeApiKey };
        }
    }
}
