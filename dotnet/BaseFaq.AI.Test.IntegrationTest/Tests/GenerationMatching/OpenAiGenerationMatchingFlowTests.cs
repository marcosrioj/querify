using BaseFaq.AI.Business.Common.Infrastructure;
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

        var tenantId = Guid.NewGuid();
        var requestedByUserId = Guid.NewGuid();
        var aiUserId = Guid.NewGuid();
        var configuration = BuildConfiguration(aiUserId);

        await using var tenantDbContext = CreateTenantDbContext(
            tenantDatabase.ConnectionString,
            tenantId,
            requestedByUserId,
            configuration);
        await tenantDbContext.Database.MigrateAsync();
        await SeedTenantProviderScenarioAsync(
            tenantDbContext,
            tenantId,
            faqDatabase.ConnectionString,
            settings);

        (Guid FaqId, Guid SimilarCandidateFaqItemId) faqScenario;
        await using (var faqSeedDbContext = CreateFaqDbContext(
                         faqDatabase.ConnectionString,
                         tenantId,
                         requestedByUserId,
                         configuration))
        {
            await faqSeedDbContext.Database.MigrateAsync();
            faqScenario = await SeedFaqScenarioAsync(faqSeedDbContext, tenantId);
        }

        var aiProviderContextResolver = new AiProviderContextResolver(tenantDbContext);
        var faqConnectionStringProvider =
            new SingleTenantConnectionStringProvider(tenantId, faqDatabase.ConnectionString);
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

        var generationPromptBuilder = new GenerationPromptBuilder();
        var contentRefStudyService = new ContentRefStudyService();
        var generationProviderClient = new GenerationProviderClient(runtimeContextResolver, textCompletionGateway);
        var matchingProviderClient =
            new MatchingProviderClient(runtimeContextResolver, embeddingsGateway, textCompletionGateway);
        var matchingExecutionService =
            new MatchingExecutionService(aiProviderContextResolver, faqDbContextFactory, matchingProviderClient);

        var generationRequested = new FaqGenerationRequestedV1
        {
            CorrelationId = Guid.NewGuid(),
            FaqId = faqScenario.FaqId,
            TenantId = tenantId,
            RequestedByUserId = requestedByUserId,
            Language = "pt-BR",
            IdempotencyKey = Guid.NewGuid().ToString("N"),
            RequestedUtc = DateTime.UtcNow
        };

        var generatedFaqItemId = Guid.Empty;
        await using (var generationDbContext = faqDbContextFactory.Create(tenantId))
        {
            var contentRefs = await generationDbContext.FaqContentRefs
                .AsNoTracking()
                .Where(x => x.FaqId == faqScenario.FaqId && x.TenantId == tenantId)
                .Select(x => new ValueTuple<ContentRefKind, string>(x.ContentRef.Kind, x.ContentRef.Locator))
                .ToListAsync();

            var studiedRefs = contentRefStudyService.Study(contentRefs);
            var generationProviderContext = await aiProviderContextResolver.ResolveAsync(
                tenantId,
                AiCommandType.Generation);
            var promptData = generationPromptBuilder.BuildPromptData(
                generationRequested,
                studiedRefs,
                generationProviderContext);
            var generatedDraft = await generationProviderClient.GenerateDraftAsync(
                generationProviderContext,
                promptData,
                CancellationToken.None);

            Assert.False(string.IsNullOrWhiteSpace(generatedDraft.Question));
            Assert.False(string.IsNullOrWhiteSpace(generatedDraft.Summary));
            Assert.False(string.IsNullOrWhiteSpace(generatedDraft.Answer));

            var generatedFaqItem = new FaqItem
            {
                Question = Truncate(generatedDraft.Question, FaqItem.MaxQuestionLength),
                ShortAnswer = Truncate(generatedDraft.Summary, FaqItem.MaxShortAnswerLength),
                Answer = Truncate(generatedDraft.Answer, FaqItem.MaxAnswerLength),
                Sort = 99,
                VoteScore = 0,
                AiConfidenceScore = Math.Clamp(generatedDraft.Confidence, 0, 100),
                IsActive = true,
                FaqId = faqScenario.FaqId,
                TenantId = tenantId
            };

            generationDbContext.FaqItems.Add(generatedFaqItem);
            await generationDbContext.SaveChangesAsync();
            generatedFaqItemId = generatedFaqItem.Id;
        }

        var matchingRequested = new FaqMatchingRequestedV1
        {
            CorrelationId = Guid.NewGuid(),
            TenantId = tenantId,
            FaqItemId = generatedFaqItemId,
            RequestedByUserId = requestedByUserId,
            Query = settings.SimilarCandidateQuestion,
            Language = "pt-BR",
            IdempotencyKey = Guid.NewGuid().ToString("N"),
            RequestedUtc = DateTime.UtcNow
        };

        var rankedCandidates = await matchingExecutionService.ExecuteAsync(
            matchingRequested,
            CancellationToken.None);

        Assert.NotEmpty(rankedCandidates);
        Assert.Equal(faqScenario.SimilarCandidateFaqItemId, rankedCandidates[0].FaqItemId);
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
                AiProviderKey = settings.ApiKey
            },
            new TenantAiProvider
            {
                TenantId = tenantId,
                AiProviderId = matchingProvider.Id,
                AiProviderKey = settings.ApiKey
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

        public string ApiKey { get; } = apiKey;
        public string GenerationModel { get; } = generationModel;
        public string MatchingModel { get; } = matchingModel;
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

            if (apiKey.Length > TenantAiProvider.MaxAiProviderKeyLength)
            {
                return false;
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
}