using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Common.EntityFramework.Tenant.Entities;
using BaseFaq.Tools.Seed.Abstractions;
using BaseFaq.Tools.Seed.Configuration;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.Tenant.Enums;
using BaseFaq.Models.User.Enums;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Tools.Seed.Application;

public sealed class TenantSeedService : ITenantSeedService
{
    private const string IaAgentName = "IA Agent";
    private const string IaAgentEmail = "iaagent@basefaq.com";
    private const string IaAgentExternalId = "iaagent@basefaq.com";

    public bool HasData(TenantDbContext dbContext)
    {
        return dbContext.Tenants.Any() ||
               dbContext.TenantConnections.Any();
    }

    public bool HasEssentialData(TenantDbContext dbContext)
    {
        var hasIaAgent = dbContext.Users.Any(item =>
            item.ExternalId == IaAgentExternalId || item.Email == IaAgentEmail);

        if (!hasIaAgent)
        {
            return false;
        }

        var requiredKeys = BuildAiProviderEntries()
            .Select(x => $"{(int)x.Command}|{x.Provider}|{x.Model}")
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var existingKeys = dbContext.AiProviders
            .AsNoTracking()
            .Select(x => new { x.Command, x.Provider, x.Model })
            .ToList()
            .Select(x => $"{(int)x.Command}|{x.Provider}|{x.Model}")
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return requiredKeys.All(existingKeys.Contains);
    }

    public Guid SeedDummyData(TenantDbContext dbContext, TenantSeedRequest request, SeedCounts counts)
    {
        var existingExternalIds = dbContext.Users.AsNoTracking().Select(user => user.ExternalId).ToHashSet();
        var existingSlugs = dbContext.Tenants.AsNoTracking().Select(tenant => tenant.Slug).ToHashSet();

        var users = BuildUsers(counts.UserCount, existingExternalIds);
        dbContext.Users.AddRange(users);
        dbContext.SaveChanges();

        var aiProviders = GetExistingAiProvidersForTenantProvisioning(dbContext);

        var tenants = BuildSingleTenant(users, existingSlugs, request, aiProviders);
        dbContext.Tenants.AddRange(tenants);
        dbContext.SaveChanges();

        var connections = BuildSingleFaqConnection(request);
        dbContext.TenantConnections.AddRange(connections);
        dbContext.SaveChanges();

        var seedTenant = tenants.FirstOrDefault(t => t.App == AppEnum.Faq && t.IsActive)
                         ?? tenants.FirstOrDefault(t => t.App == AppEnum.Faq)
                         ?? tenants.First();

        return seedTenant.Id;
    }

    public Guid EnsureEssentialData(TenantDbContext dbContext)
    {
        SeedAiProviders(dbContext);
        return EnsureIaAgentUser(dbContext);
    }

    private static Guid EnsureIaAgentUser(TenantDbContext dbContext)
    {
        var user = dbContext.Users.FirstOrDefault(item => item.ExternalId == IaAgentExternalId)
                   ?? dbContext.Users.FirstOrDefault(item => item.Email == IaAgentEmail);

        if (user is null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                GivenName = IaAgentName,
                SurName = null,
                Email = IaAgentEmail,
                ExternalId = IaAgentExternalId,
                PhoneNumber = string.Empty,
                Role = UserRoleType.Member
            };

            dbContext.Users.Add(user);
        }
        else
        {
            user.GivenName = IaAgentName;
            user.SurName = null;
            user.Email = IaAgentEmail;
            user.ExternalId = IaAgentExternalId;
            user.PhoneNumber = string.Empty;
            user.Role = UserRoleType.Member;
        }

        dbContext.SaveChanges();
        return user.Id;
    }

    private static List<User> BuildUsers(int count, HashSet<string> existingExternalIds)
    {
        var users = new List<User>();
        var index = 1;

        while (users.Count < count)
        {
            var externalId = $"seed-user-{index:000}";
            if (existingExternalIds.Contains(externalId))
            {
                index++;
                continue;
            }

            users.Add(new User
            {
                Id = Guid.NewGuid(),
                GivenName = $"User {index:000}",
                SurName = index % 3 == 0 ? null : $"Seed {index:000}",
                Email = $"user{index:000}@seed.basefaq.local",
                ExternalId = externalId,
                PhoneNumber = index % 4 == 0 ? "" : $"+1-555-01{index:000}",
                Role = index % 7 == 0 ? UserRoleType.Admin : UserRoleType.Member
            });

            index++;
        }

        return users;
    }

    private static List<Tenant> BuildSingleTenant(
        IReadOnlyList<User> users,
        HashSet<string> existingSlugs,
        TenantSeedRequest request,
        IReadOnlyList<AiProvider> aiProviders)
    {
        var slug = existingSlugs.Contains("tenant-001") ? $"tenant-{Guid.NewGuid():N}" : "tenant-001";
        var userId = users.Count > 0 ? users[0].Id : Guid.Empty;
        var generationProvider = aiProviders.First(x => x.Command == AiCommandType.Generation);
        var matchingProvider = aiProviders.First(x => x.Command == AiCommandType.Matching);
        var tenantId = Guid.NewGuid();

        return
        [
            new Tenant
            {
                Id = tenantId,
                Slug = slug,
                Name = Tenant.DefaultTenantName,
                Edition = TenantEdition.Free,
                App = AppEnum.Faq,
                ConnectionString = request.FaqConnectionString,
                IsActive = true,
                UserId = userId,
                AiProviders =
                [
                    new TenantAiProvider
                    {
                        Id = Guid.NewGuid(),
                        TenantId = tenantId,
                        AiProviderId = generationProvider.Id,
                        AiProviderKey = "seed-generation-key"
                    },
                    new TenantAiProvider
                    {
                        Id = Guid.NewGuid(),
                        TenantId = tenantId,
                        AiProviderId = matchingProvider.Id,
                        AiProviderKey = "seed-matching-key"
                    }
                ]
            }
        ];
    }

    private static List<TenantConnection> BuildSingleFaqConnection(TenantSeedRequest request)
    {
        return
        [
            new TenantConnection
            {
                Id = Guid.NewGuid(),
                ConnectionString = request.FaqConnectionString,
                App = AppEnum.Faq,
                IsCurrent = true
            }
        ];
    }

    private static List<AiProvider> GetExistingAiProvidersForTenantProvisioning(TenantDbContext dbContext)
    {
        var aiProviders = dbContext.AiProviders
            .AsNoTracking()
            .OrderBy(x => x.Command)
            .ThenBy(x => x.Provider)
            .ThenBy(x => x.Model)
            .ToList();

        if (!aiProviders.Any(x => x.Command == AiCommandType.Generation) ||
            !aiProviders.Any(x => x.Command == AiCommandType.Matching))
        {
            throw new InvalidOperationException(
                "Essential AI providers are missing. Run essential seed before dummy seed.");
        }

        return aiProviders;
    }

    private static List<AiProvider> SeedAiProviders(TenantDbContext dbContext)
    {
        var required = BuildAiProviderEntries();

        var existingKeys = dbContext.AiProviders
            .AsNoTracking()
            .Select(x => new { x.Command, x.Provider, x.Model })
            .ToList()
            .Select(x => $"{(int)x.Command}|{x.Provider}|{x.Model}")
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var toInsert = required
            .Where(x => !existingKeys.Contains($"{(int)x.Command}|{x.Provider}|{x.Model}"))
            .ToList();

        if (toInsert.Count > 0)
        {
            dbContext.AiProviders.AddRange(toInsert);
            dbContext.SaveChanges();
        }

        return dbContext.AiProviders
            .AsNoTracking()
            .OrderBy(x => x.Command)
            .ThenBy(x => x.Provider)
            .ToList();
    }

    private static List<AiProvider> BuildAiProviderEntries()
    {
        var generationModels = new (string Provider, string Model)[]
        {
            ("openai", "gpt-4o-mini"),
            ("anthropic", "claude-3-5-sonnet"),
            ("google", "gemini-1.5-pro"),
            ("azure-openai", "gpt-4o"),
            ("aws-bedrock", "anthropic.claude-3-sonnet"),
            ("cohere", "command-r-plus"),
            ("mistral", "mistral-large-latest"),
            ("together-ai", "meta-llama/Meta-Llama-3.1-70B-Instruct-Turbo"),
            ("fireworks-ai", "accounts/fireworks/models/llama-v3p1-70b-instruct"),
            ("groq", "llama-3.1-70b-versatile"),
            ("voyage-ai", "external-llm-required"),
            ("jina-ai", "external-llm-required")
        };

        var matchingModels = new (string Provider, string Model)[]
        {
            ("openai", "text-embedding-3-small"),
            ("anthropic", "external-embedding-required"),
            ("google", "text-embedding-004"),
            ("azure-openai", "text-embedding-3-large"),
            ("aws-bedrock", "amazon.titan-embed-text-v2"),
            ("cohere", "embed-multilingual-v3.0"),
            ("mistral", "mistral-embed"),
            ("together-ai", "BAAI/bge-large-en-v1.5"),
            ("fireworks-ai", "nomic-ai/nomic-embed-text-v1.5"),
            ("groq", "external-embedding-required"),
            ("voyage-ai", "voyage-3"),
            ("jina-ai", "jina-embeddings-v3")
        };

        var providers = generationModels
            .Select(entry => BuildAiProviderEntry(entry.Provider, entry.Model, AiCommandType.Generation))
            .ToList();

        providers.AddRange(
            matchingModels.Select(entry => BuildAiProviderEntry(entry.Provider, entry.Model, AiCommandType.Matching)));

        return providers;
    }

    private static AiProvider BuildAiProviderEntry(string provider, string model, AiCommandType command)
    {
        return new AiProvider
        {
            Id = Guid.NewGuid(),
            Provider = provider,
            Model = model,
            Prompt = BuildPromptByModel(command, provider, model),
            Command = command
        };
    }

    private static string BuildPromptByModel(AiCommandType command, string provider, string model)
    {
        var normalizedModel = model.Trim().ToLowerInvariant();
        var basePrompt = command switch
        {
            AiCommandType.Generation => BuildGenerationPromptBase(),
            AiCommandType.Matching => BuildMatchingPromptBase(),
            _ => "You are an AI assistant. Follow instructions and return deterministic JSON."
        };

        var modelGuidance = command switch
        {
            AiCommandType.Generation => BuildGenerationModelGuidance(provider, normalizedModel),
            AiCommandType.Matching => BuildMatchingModelGuidance(provider, normalizedModel),
            _ => "No model-specific guidance."
        };

        return
            $"{basePrompt}\n\nProvider-model profile: {provider}/{model}\nModel-specific guidance: {modelGuidance}";
    }

    private static string BuildGenerationPromptBase()
    {
        return """
               You are a multilingual FAQ generation engine.
               Objective: transform supplied reference context into a high-quality FAQ draft.
               Hard constraints:
               - Use only supplied context. Do not invent facts, numbers, policies, or citations.
               - If evidence is incomplete, keep claims conservative and add uncertainty notes.
               - Keep language aligned with the requested language.
               - Produce output that is valid against the required JSON schema.
               - Return JSON only, with no markdown fences or extra prose.
               Quality bar:
               - Question should be explicit and user-facing.
               - Summary must be concise and scannable.
               - Answer should be structured, practical, and traceable to cited references.
               - Confidence should reflect evidence quality and coverage.
               """;
    }

    private static string BuildMatchingPromptBase()
    {
        return """
               You are a FAQ semantic matching engine.
               Objective: rank candidate FAQ questions by true semantic relevance to the query.
               Hard constraints:
               - Prioritize semantic equivalence over keyword overlap.
               - Penalize partial-topic overlap and near-miss intent.
               - Prefer language-consistent candidates when meaning is otherwise similar.
               - Return deterministic results for the same inputs.
               - Output valid JSON only, no markdown or additional narrative.
               Ranking policy:
               - Include only the strongest matches.
               - Sort by score descending.
               - Keep score calibrated in [0,1] and reasons brief and specific.
               """;
    }

    private static string BuildGenerationModelGuidance(string provider, string normalizedModel)
    {
        if (provider.Equals("openai", StringComparison.OrdinalIgnoreCase) ||
            provider.Equals("azure-openai", StringComparison.OrdinalIgnoreCase) ||
            normalizedModel.Contains("gpt-4o", StringComparison.Ordinal))
        {
            return
                "Optimize for strict JSON reliability and concise factual language; avoid stylistic verbosity.";
        }

        if (normalizedModel.Contains("claude", StringComparison.Ordinal))
        {
            return
                "Keep reasoning implicit; return final JSON directly without role labels, markdown blocks, or XML-like wrappers.";
        }

        if (normalizedModel.Contains("gemini", StringComparison.Ordinal))
        {
            return
                "Avoid any explanatory preamble and ensure escaped characters keep JSON parse-safe.";
        }

        if (normalizedModel.Contains("llama", StringComparison.Ordinal) ||
            normalizedModel.Contains("instruct", StringComparison.Ordinal))
        {
            return
                "Follow instruction format strictly and suppress conversational filler around the JSON object.";
        }

        if (normalizedModel.Contains("external-llm-required", StringComparison.Ordinal))
        {
            return
                "Assume an externally selected chat-completion model; prioritize schema adherence and zero-hallucination behavior.";
        }

        return "Prioritize deterministic output, factual grounding, and strict schema adherence.";
    }

    private static string BuildMatchingModelGuidance(string provider, string normalizedModel)
    {
        if (normalizedModel.Contains("external-embedding-required", StringComparison.Ordinal))
        {
            return
                "Use this prompt as policy for downstream re-ranking when embeddings are produced by an external provider.";
        }

        if (normalizedModel.Contains("embedding", StringComparison.Ordinal) ||
            normalizedModel.Contains("embed", StringComparison.Ordinal) ||
            normalizedModel.Contains("bge", StringComparison.Ordinal) ||
            normalizedModel.Contains("voyage", StringComparison.Ordinal) ||
            normalizedModel.Contains("jina", StringComparison.Ordinal) ||
            normalizedModel.Contains("nomic", StringComparison.Ordinal))
        {
            return
                "Treat semantic closeness as primary signal and reserve high scores for near-equivalent user intent.";
        }

        if (provider.Equals("openai", StringComparison.OrdinalIgnoreCase) ||
            provider.Equals("azure-openai", StringComparison.OrdinalIgnoreCase))
        {
            return
                "Keep ranking deterministic; avoid optimistic scoring when evidence of intent match is weak.";
        }

        return "Prefer precision-first ranking, short rationale text, and stable score calibration.";
    }
}