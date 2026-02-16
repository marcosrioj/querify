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
               dbContext.Users.Any() ||
               dbContext.TenantConnections.Any() ||
               dbContext.AiProviders.Any();
    }

    public Guid SeedDummyData(TenantDbContext dbContext, TenantSeedRequest request, SeedCounts counts)
    {
        var existingExternalIds = dbContext.Users.AsNoTracking().Select(user => user.ExternalId).ToHashSet();
        var existingSlugs = dbContext.Tenants.AsNoTracking().Select(tenant => tenant.Slug).ToHashSet();

        var users = BuildUsers(counts.UserCount, existingExternalIds);
        dbContext.Users.AddRange(users);
        dbContext.SaveChanges();

        var aiProviders = SeedAiProviders(dbContext);
        var tenantAiProviderId = aiProviders.First(x => x.Command == AiCommandType.Generation).Id;

        var tenants = BuildSingleTenant(users, existingSlugs, request, tenantAiProviderId);
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

    public Guid EnsureIaAgentUser(TenantDbContext dbContext)
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
        Guid aiProviderId)
    {
        var slug = existingSlugs.Contains("tenant-001") ? $"tenant-{Guid.NewGuid():N}" : "tenant-001";
        var userId = users.Count > 0 ? users[0].Id : Guid.Empty;

        return
        [
            new Tenant
            {
                Id = Guid.NewGuid(),
                Slug = slug,
                Name = Tenant.DefaultTenantName,
                Edition = TenantEdition.Free,
                App = AppEnum.Faq,
                ConnectionString = request.FaqConnectionString,
                AiProviderId = aiProviderId,
                IsActive = true,
                UserId = userId
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
        const string generationPrompt =
            "You are a multilingual FAQ generation engine. Use only supplied source context and return schema-compliant JSON.";
        const string matchingPrompt =
            "You are a FAQ semantic matching engine. Rank by semantic similarity and return deterministic JSON.";

        return
        [
            new AiProvider
            {
                Id = Guid.NewGuid(), Provider = "openai", Model = "gpt-4o-mini", Prompt = generationPrompt,
                Command = AiCommandType.Generation
            },
            new AiProvider
            {
                Id = Guid.NewGuid(), Provider = "anthropic", Model = "claude-3-5-sonnet", Prompt = generationPrompt,
                Command = AiCommandType.Generation
            },
            new AiProvider
            {
                Id = Guid.NewGuid(), Provider = "google", Model = "gemini-1.5-pro", Prompt = generationPrompt,
                Command = AiCommandType.Generation
            },
            new AiProvider
            {
                Id = Guid.NewGuid(), Provider = "azure-openai", Model = "gpt-4o", Prompt = generationPrompt,
                Command = AiCommandType.Generation
            },
            new AiProvider
            {
                Id = Guid.NewGuid(), Provider = "aws-bedrock", Model = "anthropic.claude-3-sonnet",
                Prompt = generationPrompt, Command = AiCommandType.Generation
            },
            new AiProvider
            {
                Id = Guid.NewGuid(), Provider = "cohere", Model = "command-r-plus", Prompt = generationPrompt,
                Command = AiCommandType.Generation
            },
            new AiProvider
            {
                Id = Guid.NewGuid(), Provider = "mistral", Model = "mistral-large-latest", Prompt = generationPrompt,
                Command = AiCommandType.Generation
            },
            new AiProvider
            {
                Id = Guid.NewGuid(), Provider = "together-ai", Model = "meta-llama/Meta-Llama-3.1-70B-Instruct-Turbo",
                Prompt = generationPrompt, Command = AiCommandType.Generation
            },
            new AiProvider
            {
                Id = Guid.NewGuid(), Provider = "fireworks-ai",
                Model = "accounts/fireworks/models/llama-v3p1-70b-instruct", Prompt = generationPrompt,
                Command = AiCommandType.Generation
            },
            new AiProvider
            {
                Id = Guid.NewGuid(), Provider = "groq", Model = "llama-3.1-70b-versatile", Prompt = generationPrompt,
                Command = AiCommandType.Generation
            },
            new AiProvider
            {
                Id = Guid.NewGuid(), Provider = "voyage-ai", Model = "external-llm-required", Prompt = generationPrompt,
                Command = AiCommandType.Generation
            },
            new AiProvider
            {
                Id = Guid.NewGuid(), Provider = "jina-ai", Model = "external-llm-required", Prompt = generationPrompt,
                Command = AiCommandType.Generation
            },

            new AiProvider
            {
                Id = Guid.NewGuid(), Provider = "openai", Model = "text-embedding-3-small", Prompt = matchingPrompt,
                Command = AiCommandType.Matching
            },
            new AiProvider
            {
                Id = Guid.NewGuid(), Provider = "anthropic", Model = "external-embedding-required",
                Prompt = matchingPrompt, Command = AiCommandType.Matching
            },
            new AiProvider
            {
                Id = Guid.NewGuid(), Provider = "google", Model = "text-embedding-004", Prompt = matchingPrompt,
                Command = AiCommandType.Matching
            },
            new AiProvider
            {
                Id = Guid.NewGuid(), Provider = "azure-openai", Model = "text-embedding-3-large",
                Prompt = matchingPrompt, Command = AiCommandType.Matching
            },
            new AiProvider
            {
                Id = Guid.NewGuid(), Provider = "aws-bedrock", Model = "amazon.titan-embed-text-v2",
                Prompt = matchingPrompt, Command = AiCommandType.Matching
            },
            new AiProvider
            {
                Id = Guid.NewGuid(), Provider = "cohere", Model = "embed-multilingual-v3.0", Prompt = matchingPrompt,
                Command = AiCommandType.Matching
            },
            new AiProvider
            {
                Id = Guid.NewGuid(), Provider = "mistral", Model = "mistral-embed", Prompt = matchingPrompt,
                Command = AiCommandType.Matching
            },
            new AiProvider
            {
                Id = Guid.NewGuid(), Provider = "together-ai", Model = "BAAI/bge-large-en-v1.5",
                Prompt = matchingPrompt, Command = AiCommandType.Matching
            },
            new AiProvider
            {
                Id = Guid.NewGuid(), Provider = "fireworks-ai", Model = "nomic-ai/nomic-embed-text-v1.5",
                Prompt = matchingPrompt, Command = AiCommandType.Matching
            },
            new AiProvider
            {
                Id = Guid.NewGuid(), Provider = "groq", Model = "external-embedding-required", Prompt = matchingPrompt,
                Command = AiCommandType.Matching
            },
            new AiProvider
            {
                Id = Guid.NewGuid(), Provider = "voyage-ai", Model = "voyage-3", Prompt = matchingPrompt,
                Command = AiCommandType.Matching
            },
            new AiProvider
            {
                Id = Guid.NewGuid(), Provider = "jina-ai", Model = "jina-embeddings-v3", Prompt = matchingPrompt,
                Command = AiCommandType.Matching
            }
        ];
    }
}