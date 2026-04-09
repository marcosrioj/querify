using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Common.EntityFramework.Core.Entities;
using BaseFaq.Common.EntityFramework.Tenant.Entities;
using BaseFaq.Common.EntityFramework.Tenant.Helpers;
using BaseFaq.Tools.Seed.Abstractions;
using BaseFaq.Tools.Seed.Configuration;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.Tenant.Enums;
using BaseFaq.Models.User.Enums;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Tools.Seed.Application;

public sealed class TenantSeedService : ITenantSeedService
{
    private const string IaAgentName = "AI Agent";
    private const string IaAgentEmail = "iaagent@basefaq.com";
    private const string IaAgentExternalId = "iaagent@basefaq.com";
    private const string SeedTenantSlug = "tenant-001";
    private const string SeedGenerationProviderKey = "seed-generation-key";
    private const string SeedMatchingProviderKey = "seed-matching-key";

    public bool HasEssentialData(TenantDbContext dbContext, TenantSeedRequest request, SeedCounts counts)
    {
        var hasIaAgent = dbContext.Users.Any(item =>
            item.ExternalId == IaAgentExternalId || item.Email == IaAgentEmail);

        if (!hasIaAgent)
        {
            return false;
        }

        var requiredProviders = BuildAiProviderEntries();
        var existingByKey = dbContext.AiProviders
            .AsNoTracking()
            .ToList()
            .GroupBy(x => BuildAiProviderKey(x.Command, x.Provider), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.ToList(), StringComparer.OrdinalIgnoreCase);

        var hasRequiredProviders = requiredProviders.All(requiredProvider =>
        {
            var key = BuildAiProviderKey(requiredProvider.Command, requiredProvider.Provider);
            return existingByKey.TryGetValue(key, out var providers) &&
                   providers.Any(x => x.Model.Equals(requiredProvider.Model, StringComparison.OrdinalIgnoreCase));
        });

        if (!hasRequiredProviders)
        {
            return false;
        }

        var normalizedUserCount = NormalizeSeedUserCount(counts.UserCount);
        var expectedSeedUserExternalIds = Enumerable
            .Range(1, normalizedUserCount)
            .Select(BuildSeedUserExternalId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var seedUsers = dbContext.Users
            .AsNoTracking()
            .Where(user => expectedSeedUserExternalIds.Contains(user.ExternalId))
            .ToDictionary(user => user.ExternalId, StringComparer.OrdinalIgnoreCase);

        if (seedUsers.Count != expectedSeedUserExternalIds.Count)
        {
            return false;
        }

        var seedTenant = dbContext.Tenants
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Include(tenant => tenant.TenantUsers)
            .Include(tenant => tenant.AiProviders)
            .ThenInclude(tenantAiProvider => tenantAiProvider.AiProvider)
            .FirstOrDefault(tenant => tenant.Slug == SeedTenantSlug);

        if (seedTenant is null ||
            seedTenant.IsDeleted ||
            !seedTenant.IsActive ||
            seedTenant.App != AppEnum.Faq ||
            !string.Equals(seedTenant.ConnectionString, request.FaqConnectionString, StringComparison.Ordinal))
        {
            return false;
        }

        var ownerUser = seedUsers[BuildSeedUserExternalId(1)];
        var hasOwnerMembership = seedTenant.TenantUsers.Any(tenantUser =>
            !tenantUser.IsDeleted &&
            tenantUser.UserId == ownerUser.Id &&
            tenantUser.Role == TenantUserRoleType.Owner);

        if (!hasOwnerMembership)
        {
            return false;
        }

        if (normalizedUserCount > 1)
        {
            var memberUser = seedUsers[BuildSeedUserExternalId(2)];
            var hasMemberMembership = seedTenant.TenantUsers.Any(tenantUser =>
                !tenantUser.IsDeleted &&
                tenantUser.UserId == memberUser.Id &&
                tenantUser.Role == TenantUserRoleType.Member);

            if (!hasMemberMembership)
            {
                return false;
            }
        }

        var activeProviders = dbContext.AiProviders
            .AsNoTracking()
            .ToList();
        var generationProvider = PickPreferredProvider(activeProviders, AiCommandType.Generation);
        var matchingProvider = PickPreferredProvider(activeProviders, AiCommandType.Matching);
        var hasTenantGenerationProvider = seedTenant.AiProviders.Any(tenantAiProvider =>
            !tenantAiProvider.IsDeleted &&
            tenantAiProvider.AiProviderId == generationProvider.Id &&
            string.Equals(tenantAiProvider.AiProviderKey, SeedGenerationProviderKey, StringComparison.Ordinal));
        var hasTenantMatchingProvider = seedTenant.AiProviders.Any(tenantAiProvider =>
            !tenantAiProvider.IsDeleted &&
            tenantAiProvider.AiProviderId == matchingProvider.Id &&
            string.Equals(tenantAiProvider.AiProviderKey, SeedMatchingProviderKey, StringComparison.Ordinal));

        if (!hasTenantGenerationProvider || !hasTenantMatchingProvider)
        {
            return false;
        }

        return dbContext.TenantConnections
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Any(connection =>
                !connection.IsDeleted &&
                connection.App == AppEnum.Faq &&
                connection.IsCurrent &&
                connection.ConnectionString == request.FaqConnectionString);
    }

    public EssentialSeedResult EnsureEssentialData(TenantDbContext dbContext, TenantSeedRequest request, SeedCounts counts)
    {
        var aiProviders = SeedAiProviders(dbContext);
        var aiAgentUserId = EnsureIaAgentUser(dbContext);
        var seedUsers = EnsureSeedUsers(dbContext, counts.UserCount);
        var seedTenant = EnsureSeedTenant(dbContext, request, seedUsers, aiProviders);
        EnsureCurrentFaqConnection(dbContext, request);

        dbContext.SaveChanges();

        return new EssentialSeedResult(aiAgentUserId, seedTenant.Id);
    }

    private static Guid EnsureIaAgentUser(TenantDbContext dbContext)
    {
        var user = dbContext.Users
                       .IgnoreQueryFilters()
                       .FirstOrDefault(item => item.ExternalId == IaAgentExternalId)
                   ?? dbContext.Users
                       .IgnoreQueryFilters()
                       .FirstOrDefault(item => item.Email == IaAgentEmail);

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
            RestoreEntity(user);
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

    private static List<User> EnsureSeedUsers(TenantDbContext dbContext, int count)
    {
        var normalizedCount = NormalizeSeedUserCount(count);
        var expectedExternalIds = Enumerable
            .Range(1, normalizedCount)
            .Select(BuildSeedUserExternalId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var existingUsers = dbContext.Users
            .IgnoreQueryFilters()
            .Where(user => expectedExternalIds.Contains(user.ExternalId))
            .ToDictionary(user => user.ExternalId, StringComparer.OrdinalIgnoreCase);
        var users = new List<User>();

        for (var index = 1; index <= normalizedCount; index++)
        {
            var externalId = BuildSeedUserExternalId(index);
            if (!existingUsers.TryGetValue(externalId, out var user))
            {
                user = new User
                {
                    Id = Guid.NewGuid(),
                    GivenName = $"User {index:000}",
                    Email = $"user{index:000}@seed.basefaq.local",
                    ExternalId = externalId
                };

                dbContext.Users.Add(user);
            }

            RestoreEntity(user);
            ApplySeedUserValues(user, index);
            users.Add(user);
        }

        return users;
    }

    private static Tenant EnsureSeedTenant(
        TenantDbContext dbContext,
        TenantSeedRequest request,
        IReadOnlyList<User> users,
        IReadOnlyList<AiProvider> aiProviders)
    {
        var ownerUser = users.FirstOrDefault();
        var memberUser = users.Skip(1).FirstOrDefault();
        var generationProvider = PickPreferredProvider(aiProviders, AiCommandType.Generation);
        var matchingProvider = PickPreferredProvider(aiProviders, AiCommandType.Matching);

        var tenant = dbContext.Tenants
            .IgnoreQueryFilters()
            .Include(entity => entity.TenantUsers)
            .Include(entity => entity.AiProviders)
            .ThenInclude(entity => entity.AiProvider)
            .FirstOrDefault(entity => entity.Slug == SeedTenantSlug);

        if (tenant is null)
        {
            tenant = new Tenant
            {
                Id = Guid.NewGuid(),
                Slug = SeedTenantSlug,
                Name = Tenant.DefaultTenantName,
                Edition = TenantEdition.Free,
                App = AppEnum.Faq,
                ConnectionString = request.FaqConnectionString
            };

            dbContext.Tenants.Add(tenant);
        }

        RestoreEntity(tenant);
        tenant.Slug = SeedTenantSlug;
        tenant.Name = Tenant.DefaultTenantName;
        tenant.Edition = TenantEdition.Free;
        tenant.App = AppEnum.Faq;
        tenant.ConnectionString = request.FaqConnectionString;
        tenant.IsActive = true;

        if (ownerUser is not null)
        {
            TenantUserHelper.SetOwner(tenant.TenantUsers, tenant.Id, ownerUser.Id);
        }

        if (memberUser is not null && memberUser.Id != ownerUser?.Id)
        {
            var existingMember = tenant.TenantUsers
                .FirstOrDefault(tenantUser => tenantUser.UserId == memberUser.Id);

            if (existingMember is null)
            {
                tenant.TenantUsers.Add(new TenantUser
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenant.Id,
                    UserId = memberUser.Id,
                    Role = TenantUserRoleType.Member
                });
            }
            else
            {
                RestoreEntity(existingMember);
                existingMember.Role = TenantUserRoleType.Member;
            }
        }

        EnsureTenantAiProvider(tenant, generationProvider, SeedGenerationProviderKey, AiCommandType.Generation);
        EnsureTenantAiProvider(tenant, matchingProvider, SeedMatchingProviderKey, AiCommandType.Matching);

        return tenant;
    }

    private static AiProvider PickPreferredProvider(IReadOnlyList<AiProvider> aiProviders, AiCommandType command)
    {
        return aiProviders.FirstOrDefault(x =>
                   x.Command == command && x.Provider.Equals("openai", StringComparison.OrdinalIgnoreCase))
               ?? aiProviders.First(x => x.Command == command);
    }

    private static void EnsureCurrentFaqConnection(TenantDbContext dbContext, TenantSeedRequest request)
    {
        var connection = dbContext.TenantConnections
            .IgnoreQueryFilters()
            .FirstOrDefault(item =>
                item.App == AppEnum.Faq &&
                (item.IsCurrent || item.ConnectionString == request.FaqConnectionString));

        if (connection is null)
        {
            connection = new TenantConnection
            {
                Id = Guid.NewGuid(),
                App = AppEnum.Faq,
                ConnectionString = request.FaqConnectionString,
                IsCurrent = true
            };

            dbContext.TenantConnections.Add(connection);
            return;
        }

        RestoreEntity(connection);
        connection.ConnectionString = request.FaqConnectionString;
        connection.App = AppEnum.Faq;
        connection.IsCurrent = true;
    }

    private static List<AiProvider> SeedAiProviders(TenantDbContext dbContext)
    {
        var required = BuildAiProviderEntries();
        var existingProviders = dbContext.AiProviders
            .IgnoreQueryFilters()
            .ToList()
            .GroupBy(x => BuildAiProviderKey(x.Command, x.Provider), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.ToList(), StringComparer.OrdinalIgnoreCase);
        var assignedProviderIds = dbContext.TenantAiProviders
            .IgnoreQueryFilters()
            .Select(x => x.AiProviderId)
            .ToHashSet();

        var hasChanges = false;
        foreach (var requiredProvider in required)
        {
            var key = BuildAiProviderKey(requiredProvider.Command, requiredProvider.Provider);
            if (existingProviders.TryGetValue(key, out var providersByKey) && providersByKey.Count > 0)
            {
                var providerToUpdate =
                    providersByKey.FirstOrDefault(x =>
                        x.Model.Equals(requiredProvider.Model, StringComparison.OrdinalIgnoreCase))
                    ?? providersByKey
                        .OrderByDescending(x => assignedProviderIds.Contains(x.Id))
                        .ThenBy(x => x.IsDeleted)
                        .ThenBy(x => x.CreatedDate ?? DateTime.MinValue)
                        .First();

                if (ShouldUpdateProvider(providerToUpdate, requiredProvider))
                {
                    providerToUpdate.Provider = requiredProvider.Provider;
                    providerToUpdate.Model = requiredProvider.Model;
                    providerToUpdate.Prompt = requiredProvider.Prompt;
                    providerToUpdate.Command = requiredProvider.Command;
                    providerToUpdate.IsDeleted = false;
                    providerToUpdate.DeletedBy = null;
                    providerToUpdate.DeletedDate = null;
                    hasChanges = true;
                }

                continue;
            }

            dbContext.AiProviders.Add(requiredProvider);
            hasChanges = true;
        }

        if (hasChanges)
        {
            dbContext.SaveChanges();
        }

        return dbContext.AiProviders
            .AsNoTracking()
            .OrderBy(x => x.Command)
            .ThenBy(x => x.Provider)
            .ToList();
    }

    private static string BuildSeedUserExternalId(int index)
    {
        return $"seed-user-{index:000}";
    }

    private static int NormalizeSeedUserCount(int count)
    {
        return Math.Max(count, 1);
    }

    private static void ApplySeedUserValues(User user, int index)
    {
        user.GivenName = $"User {index:000}";
        user.SurName = index % 3 == 0 ? null : $"Seed {index:000}";
        user.Email = $"user{index:000}@seed.basefaq.local";
        user.ExternalId = BuildSeedUserExternalId(index);
        user.PhoneNumber = index % 4 == 0 ? string.Empty : $"+1-555-01{index:000}";
        user.Role = index % 7 == 0 ? UserRoleType.Admin : UserRoleType.Member;
    }

    private static void EnsureTenantAiProvider(
        Tenant tenant,
        AiProvider provider,
        string providerKey,
        AiCommandType command)
    {
        var tenantAiProvider = tenant.AiProviders
            .FirstOrDefault(item => item.AiProviderId == provider.Id)
            ?? tenant.AiProviders.FirstOrDefault(item => item.AiProvider?.Command == command);

        if (tenantAiProvider is null)
        {
            tenant.AiProviders.Add(new TenantAiProvider
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                AiProviderId = provider.Id,
                AiProviderKey = providerKey
            });

            return;
        }

        RestoreEntity(tenantAiProvider);
        tenantAiProvider.TenantId = tenant.Id;
        tenantAiProvider.AiProviderId = provider.Id;
        tenantAiProvider.AiProviderKey = providerKey;
    }

    private static void RestoreEntity(BaseEntity entity)
    {
        entity.IsDeleted = false;
        entity.DeletedBy = null;
        entity.DeletedDate = null;
    }

    private static string BuildAiProviderKey(AiCommandType command, string provider)
    {
        return $"{(int)command}|{provider}";
    }

    private static bool ShouldUpdateProvider(AiProvider existingProvider, AiProvider requiredProvider)
    {
        return !existingProvider.Provider.Equals(requiredProvider.Provider, StringComparison.Ordinal) ||
               !existingProvider.Model.Equals(requiredProvider.Model, StringComparison.Ordinal) ||
               !existingProvider.Prompt.Equals(requiredProvider.Prompt, StringComparison.Ordinal) ||
               existingProvider.Command != requiredProvider.Command ||
               existingProvider.IsDeleted ||
               existingProvider.DeletedBy is not null ||
               existingProvider.DeletedDate is not null;
    }

    private static List<AiProvider> BuildAiProviderEntries()
    {
        var generationModels = new (string Provider, string Model)[]
        {
            ("openai", "gpt-5.2"),
            ("anthropic", "claude-sonnet-4-5"),
            ("google", "gemini-2.5-pro"),
            ("azure-openai", "gpt-5.2"),
            ("aws-bedrock", "amazon.nova-pro-v1:0"),
            ("cohere", "command-a-03-2025"),
            ("mistral", "mistral-medium-latest"),
            ("together-ai", "meta-llama/Llama-3.3-70B-Instruct-Turbo"),
            ("fireworks-ai", "accounts/fireworks/models/llama-v3p3-70b-instruct"),
            ("groq", "llama-3.3-70b-versatile"),
            ("voyage-ai", "external-llm-required"),
            ("jina-ai", "external-llm-required")
        };

        var matchingModels = new (string Provider, string Model)[]
        {
            ("openai", "text-embedding-3-large"),
            ("anthropic", "external-embedding-required"),
            ("google", "gemini-embedding-001"),
            ("azure-openai", "text-embedding-3-large"),
            ("aws-bedrock", "amazon.titan-embed-text-v2:0"),
            ("cohere", "embed-multilingual-v3.0"),
            ("mistral", "mistral-embed"),
            ("together-ai", "intfloat/multilingual-e5-large-instruct"),
            ("fireworks-ai", "fireworks/qwen3-embedding-8b"),
            ("groq", "external-embedding-required"),
            ("voyage-ai", "voyage-3.5"),
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
