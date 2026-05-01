# BaseFAQ MCP: Multi-Agent Architecture

## Purpose

This document is the complete design reference for `BaseFaq.MCP.Server` — a single native .NET
MCP server that exposes all BaseFAQ modules as AI-callable tools. Different AI agents connect to
it with different tool subsets and system prompts, each operating as a specialist for one BaseFAQ
module.

**Status:** designed, not yet built. See [`../README.md`](../README.md).

---

## Core concept: one server, N module agents

An MCP server is an API for AI. Just as the BaseFAQ REST surface is consumed by the Portal
frontend and external clients, `BaseFaq.MCP.Server` is consumed by AI agent instances. Each agent
is an AI model (Claude, GPT, or any MCP-compatible model) configured with:

1. A connection to `BaseFaq.MCP.Server`
2. A module-specific MCP **prompt** that defines its role, behavior, and scope
3. A filtered **tool set** matching its module responsibilities

The same server instance serves all agents. Tool groups are prefixed by module name. QnA tools
are available to all agents because QnA is the shared knowledge source for every module.

```
┌──────────────────────────────────────────────────────────────────────┐
│ BaseFaq.MCP.Server  (one .NET process, part of BaseFaq.sln)          │
│                                                                        │
│  Tool groups:  [qna_*]  [direct_*]  [broadcast_*]  [trust_*]         │
│                [tenant_*]                                              │
│                                                                        │
│  MCP prompts:  qna_assistant  direct_assistant  broadcast_assistant   │
│                trust_assistant  tenant_assistant                       │
└────────────────────────────┬─────────────────────────────────────────┘
                             │ in-process MediatR calls
                             ▼
  BaseFaq.QnA.*   BaseFaq.Direct.*   BaseFaq.Broadcast.*
  BaseFaq.Trust.* BaseFaq.Tenant.*
```

```
AI Client                    Tool filter         Agent role
─────────────────────────────────────────────────────────────────
Claude Desktop               qna_* + tenant_*    Knowledge assistant
Claude Code (editor agent)   qna_*               Docs / Q&A helper
Automated support agent      direct_* + qna_*    Conversation resolver
Community bot                broadcast_* + qna_* Public response coordinator
Governance assistant         trust_* + qna_*     Proposal and vote manager
Ops assistant                tenant_*            Workspace operations
```

---

## Why native .NET, not a TypeScript proxy

Calling handlers directly via MediatR instead of through HTTP gives:

- **Transactional integrity** — all entities from one generation run share one `SaveChanges()`
- **No serialization overhead** — tool results are typed .NET objects, not HTTP response strings
- **Shared DI** — same services, same `DbContext`, same business rules as every other module
- **Tenant context via `ISessionService`** — identical to how handlers work in request flows
- **One deployment** — MCP server ships with the backend stack, not as a separate process

The TypeScript proxy (described in the reference document) is valid for prototyping and for
connecting external tools when the .NET stack is not accessible. For production, native .NET is
the right path.

---

## Tenant context: `McpSessionService`

Every handler that touches tenant data calls `ISessionService.GetTenantId(module)`. In HTTP hosts,
`SessionService` reads from the JWT and `X-Tenant-Id` header. In the MCP server, a custom
implementation reads from a scoped context populated at the start of each tool invocation.

```csharp
// dotnet/BaseFaq.MCP.Server/Infrastructure/McpSessionContext.cs
public class McpSessionContext
{
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public string? UserName { get; set; }
}

// dotnet/BaseFaq.MCP.Server/Infrastructure/McpSessionService.cs
public sealed class McpSessionService(McpSessionContext ctx) : ISessionService
{
    public Guid GetTenantId(ModuleEnum module) => ctx.TenantId;
    public Guid GetUserId() => ctx.UserId;
    public string? GetUserName() => ctx.UserName;
}
```

Each tool that writes tenant data accepts a `tenant_id` parameter and populates `McpSessionContext`
before dispatching commands. Read-only tools aimed at a single-tenant deployment can use a fixed
tenant from server configuration. Multi-tenant deployments always pass `tenant_id` explicitly.

```csharp
// Pattern used by every write tool
private void SetTenantContext(Guid tenantId)
{
    _session.TenantId = tenantId;
    _session.UserId = _options.ServiceUserId;
    _session.UserName = _options.ServiceUserName;
}
```

This is identical to how `TenantWorkerSessionService` provides a non-request session for the
worker host — no new pattern, same existing solution.

---

## Project structure

```
dotnet/BaseFaq.MCP.Server/
  Program.cs
  Infrastructure/
    McpSessionService.cs
    McpSessionContext.cs
    ContentFetcher.cs               HTTP + HTML extraction for Source ingestion
  Services/
    QnAGenerator.cs                 Anthropic SDK — generates Q&A from source text
  Tools/
    QnATools.cs                     qna_* tools (read + write + import)
    DirectTools.cs                  direct_* tools (needs Direct API surface)
    BroadcastTools.cs               broadcast_* tools (needs Broadcast API surface)
    TrustTools.cs                   trust_* tools (needs Trust module)
    TenantTools.cs                  tenant_* tools
  Prompts/
    AgentPrompts.cs                 one MCP prompt per agent type
  Options/
    McpServerOptions.cs
  Extensions/
    ServiceCollectionExtensions.cs
```

### `Program.cs`

```csharp
var builder = Host.CreateApplicationBuilder(args);

// Reuse existing feature registrations
builder.Services
    .AddQnAPortalFeatures(builder.Configuration)
    .AddQnAPublicFeatures(builder.Configuration)
    .AddTenantPortalFeatures(builder.Configuration);

// Replace HTTP-bound session with MCP context-aware session
builder.Services.AddScoped<McpSessionContext>();
builder.Services.AddScoped<ISessionService, McpSessionService>();

// Anthropic SDK for AI generation tools
builder.Services.AddSingleton(
    new AnthropicClient(builder.Configuration["Anthropic:ApiKey"]!));

builder.Services.Configure<McpServerOptions>(
    builder.Configuration.GetSection("McpServer"));

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()          // swap for SSE when hosting over HTTP
    .WithToolsFromAssembly(typeof(QnATools).Assembly)
    .WithPromptsFromAssembly(typeof(AgentPrompts).Assembly);

await builder.Build().RunAsync();
```

---

## Tool groups

### `qna_*` — Knowledge base tools

Available to every agent. QnA is the shared knowledge source across all modules.

| Tool | Handler / Route today | Status |
|---|---|---|
| `qna_list_spaces` | `GetSpacesQuery` | ✅ today |
| `qna_get_space` | `GetSpaceBySlugQuery` | ✅ today |
| `qna_list_questions` | `GetQuestionsQuery` | ✅ today |
| `qna_get_question` | `GetQuestionQuery` | ✅ today |
| `qna_search` | needs `BaseFaq.QnA.Public.Business.Search` | ❌ Gap 3 |
| `qna_create_question` | `CreateQuestionCommand` | ✅ today |
| `qna_create_answer` | `CreateAnswerCommand` | ✅ today |
| `qna_activate_answer` | `ActivateAnswerCommand` | ✅ today |
| `qna_create_source` | `CreateSourceCommand` | ✅ today |
| `qna_link_source` | `AddQuestionSourceCommand` / `AddAnswerSourceCommand` | ✅ today |
| `qna_import_source` | needs `GenerateQnAFromSourceCommand` | ❌ Gap 4 |

```csharp
[McpServerToolType]
public sealed class QnATools(
    IMediator mediator,
    McpSessionContext session,
    QnAGenerator generator,
    ContentFetcher fetcher,
    IOptions<McpServerOptions> options)
{
    [McpServerTool(Name = "qna_list_spaces",
        Description = "List all published knowledge spaces for the tenant.")]
    public async Task<string> ListSpaces(
        [McpServerToolParameter(Description = "Tenant ID")] Guid tenantId,
        CancellationToken ct)
    {
        SetTenantContext(tenantId);
        var result = await mediator.Send(new GetSpacesQuery { Visibility = VisibilityScope.Public }, ct);
        return Serialize(result);
    }

    [McpServerTool(Name = "qna_get_question",
        Description = "Get a question with its answers.")]
    public async Task<string> GetQuestion(
        [McpServerToolParameter(Description = "Tenant ID")] Guid tenantId,
        [McpServerToolParameter(Description = "Question ID")] Guid questionId,
        CancellationToken ct)
    {
        SetTenantContext(tenantId);
        var result = await mediator.Send(new GetQuestionQuery { QuestionId = questionId }, ct);
        return Serialize(result);
    }

    [McpServerTool(Name = "qna_create_question",
        Description = "Create a new question as a Draft. Requires write access.")]
    public async Task<string> CreateQuestion(
        [McpServerToolParameter] Guid tenantId,
        [McpServerToolParameter] Guid spaceId,
        [McpServerToolParameter(Description = "Canonical question text")] string title,
        [McpServerToolParameter] string? summary,
        [McpServerToolParameter(Description = "AI confidence 0-1")] decimal aiConfidenceScore = 0,
        CancellationToken ct = default)
    {
        SetTenantContext(tenantId);
        var id = await mediator.Send(new CreateQuestionCommand
        {
            SpaceId = spaceId,
            Title = title,
            Summary = summary,
            Status = QuestionStatus.Draft,
            Visibility = VisibilityScope.Internal,
            OriginChannel = ChannelKind.Api,    // Gap 2: use dedicated value when added
            AiConfidenceScore = aiConfidenceScore,
        }, ct);
        return Serialize(new { id });
    }

    // ... other tools follow the same pattern

    private void SetTenantContext(Guid tenantId)
    {
        session.TenantId = tenantId;
        session.UserId = options.Value.ServiceUserId;
    }
}
```

---

### `direct_*` — Conversation resolution tools

Used by the Direct agent. Also uses `qna_search` to ground responses in the knowledge base.

Requires `BaseFaq.Direct.*` API surface to exist. Currently only the persistence boundary
(`BaseFaq.Direct.Common.Persistence.DirectDb`) is implemented.

| Tool | Needs | Status |
|---|---|---|
| `direct_list_conversations` | `GetConversationsQuery` in Direct Portal | ❌ needs Direct API |
| `direct_get_conversation` | `GetConversationQuery` | ❌ needs Direct API |
| `direct_send_message` | `SendMessageCommand` | ❌ needs Direct API |
| `direct_resolve` | `ResolveConversationCommand` | ❌ needs Direct API |
| `direct_escalate` | `EscalateConversationCommand` | ❌ needs Direct API |
| `direct_flag_for_qna` | `FlagConversationGapCommand` | ❌ needs Direct API |

`direct_flag_for_qna` is the cross-module handoff tool: when the Direct agent finds a gap (no
answer exists), it flags the conversation so the QnA Agent can draft a canonical answer.

```csharp
[McpServerToolType]
public sealed class DirectTools(IMediator mediator, McpSessionContext session)
{
    // Stubs — implementations added when BaseFaq.Direct.Portal.* ships
    [McpServerTool(Name = "direct_list_conversations",
        Description = "List conversations for the tenant. Requires Direct module.")]
    public Task<string> ListConversations(Guid tenantId, CancellationToken ct)
        => throw new NotImplementedException("Direct module API surface not yet available.");
}
```

---

### `broadcast_*` — Community and public channel tools

Used by the Broadcast agent. Also uses `qna_search` to find canonical answers before replying.

Requires `BaseFaq.Broadcast.*` API surface to exist. Currently only the persistence boundary
(`BaseFaq.Broadcast.Common.Persistence.BroadcastDb`) is implemented.

| Tool | Needs | Status |
|---|---|---|
| `broadcast_list_threads` | `GetThreadsQuery` in Broadcast Portal | ❌ needs Broadcast API |
| `broadcast_get_thread` | `GetThreadQuery` | ❌ needs Broadcast API |
| `broadcast_post_reply` | `PostReplyCommand` | ❌ needs Broadcast API |
| `broadcast_capture` | `CaptureThreadCommand` | ❌ needs Broadcast API |
| `broadcast_flag_for_qna` | `FlagThreadGapCommand` | ❌ needs Broadcast API |

`broadcast_flag_for_qna` is the cross-module handoff tool for the Broadcast agent: a recurring
public question flags a gap that the QnA Agent should fill with a canonical answer.

---

### `trust_*` — Governance and validation tools

Used by the Trust agent. References QnA to publish decisions as canonical answers.

Requires `BaseFaq.Trust.*` implementation. No active project exists yet.

| Tool | Status |
|---|---|
| `trust_list_proposals` | ❌ needs Trust module |
| `trust_get_proposal` | ❌ needs Trust module |
| `trust_create_proposal` | ❌ needs Trust module |
| `trust_cast_vote` | ❌ needs Trust module |
| `trust_publish_decision` | ❌ needs Trust module |

`trust_publish_decision` is the cross-module handoff to QnA: once a Trust decision is final, this
tool creates a canonical QnA answer with `SourceRole.Evidence` linking back to the Trust record.

---

### `tenant_*` — Workspace operations tools

Used by the Tenant agent. Available today via `BaseFaq.Tenant.Portal.Api`.

| Tool | Route today | Status |
|---|---|---|
| `tenant_get_workspace` | `GET /api/tenant/tenants/get-all` | ✅ today |
| `tenant_list_members` | `GET /api/tenant/tenant-users/get-all` | ✅ today |
| `tenant_get_profile` | `GET /api/user/user-profile` | ✅ today |
| `tenant_get_billing` | `GET /api/tenant/billing/summary` | ✅ today |
| `tenant_get_subscription` | `GET /api/tenant/billing/subscription` | ✅ today |
| `tenant_get_client_key` | `GET /api/tenant/tenants/get-client-key` | ✅ today |

Tenant tools use the Portal API over HTTP (or via direct handler calls when Portal handlers are
registered in the same DI container as the MCP server). For the native .NET approach, register
`AddTenantPortalFeatures(...)` in `Program.cs` and call handlers directly.

---

## Agent prompts

Each agent type has a dedicated MCP prompt registered in `AgentPrompts.cs`. An AI client loads
the prompt at session start to configure its behavior.

```csharp
[McpServerPromptType]
public static class AgentPrompts
{
    [McpServerPrompt(Name = "qna_assistant",
        Description = "Configures the AI as a BaseFAQ knowledge base specialist.")]
    public static PromptMessage[] QnAAssistant() =>
    [
        new()
        {
            Role = "user",
            Content = new TextContent("""
                You are a knowledge base specialist powered by BaseFAQ.
                Your job is to find, create, and improve question-and-answer pairs.

                Before answering any question:
                1. Call qna_search to check whether an answer already exists.
                2. If it does, use that answer and cite the space and question ID.
                3. If it does not, say so clearly and offer to create a draft question
                   and answer using qna_create_question and qna_create_answer.

                All content you create must be Draft status.
                Never publish content directly — human curation is required.
                Set aiConfidenceScore based on how well the source supports the answer.
                """),
        },
    ];

    [McpServerPrompt(Name = "direct_assistant",
        Description = "Configures the AI as a Direct conversation resolution agent.")]
    public static PromptMessage[] DirectAssistant() =>
    [
        new()
        {
            Role = "user",
            Content = new TextContent("""
                You are a private conversation resolution agent powered by BaseFAQ Direct.
                You handle 1:1 support conversations.

                For each conversation:
                1. Call direct_get_conversation to understand the context.
                2. Call qna_search to find a canonical answer.
                3. If an answer exists, draft a response using it and cite the source.
                4. If no answer exists, call direct_flag_for_qna to create a knowledge gap.
                5. If the case requires human judgment, call direct_escalate.

                Never send a response without first checking the knowledge base.
                Never handle sensitive topics (legal, billing disputes, personal data) autonomously
                — always escalate those.
                """),
        },
    ];

    [McpServerPrompt(Name = "broadcast_assistant",
        Description = "Configures the AI as a public channel response coordinator.")]
    public static PromptMessage[] BroadcastAssistant() =>
    [
        new()
        {
            Role = "user",
            Content = new TextContent("""
                You are a community and public channel response coordinator powered by BaseFAQ.
                You handle comments, mentions, and public discussions.

                For each thread:
                1. Call broadcast_get_thread to understand the context.
                2. Call qna_search to find the canonical answer.
                3. If an answer exists, post a reply using broadcast_post_reply, citing the source.
                4. If no answer exists, call broadcast_flag_for_qna to create a gap.
                5. If the topic is sensitive or requires personal information, do not reply publicly
                   — flag for human review or hand off to Direct.

                Always use the canonical QnA answer. Do not improvise public responses.
                """),
        },
    ];

    [McpServerPrompt(Name = "trust_assistant",
        Description = "Configures the AI as a governance and validation assistant.")]
    public static PromptMessage[] TrustAssistant() =>
    [
        new()
        {
            Role = "user",
            Content = new TextContent("""
                You are a governance assistant powered by BaseFAQ Trust.
                You help manage proposals, votes, and formal decisions.

                For each governance task:
                1. Check existing proposals with trust_list_proposals before creating a new one.
                2. Summarize the proposal context clearly before casting or suggesting a vote.
                3. When a decision is final, call trust_publish_decision to create a canonical
                   QnA answer so the outcome is reusable.

                You record decisions, not make them. Always present options and evidence;
                never vote autonomously unless the human explicitly delegates that authority.
                """),
        },
    ];

    [McpServerPrompt(Name = "tenant_assistant",
        Description = "Configures the AI as a workspace operations assistant.")]
    public static PromptMessage[] TenantAssistant() =>
    [
        new()
        {
            Role = "user",
            Content = new TextContent("""
                You are a workspace operations assistant powered by BaseFAQ.
                You help with workspace setup, member management, and billing questions.

                Use tenant_get_workspace to understand the current workspace state.
                Use tenant_list_members to answer questions about team access.
                Use tenant_get_billing and tenant_get_subscription for billing inquiries.

                You are read-only unless the human explicitly asks you to make a change.
                Never generate new API keys or modify permissions without explicit confirmation.
                """),
        },
    ];
}
```

---

## Connecting AI clients

### Per-agent configuration pattern

Each agent is a separate entry in the MCP client configuration. All entries point to the same
server binary; only the prompt and (optionally) the exposed tool filter differ.

### Claude Desktop

`~/Library/Application Support/Claude/claude_desktop_config.json`:

```json
{
  "mcpServers": {
    "basefaq-qna": {
      "command": "dotnet",
      "args": ["run", "--project", "/path/to/BaseFaq.MCP.Server", "--no-build"],
      "env": {
        "ASPNETCORE_ENVIRONMENT": "Development",
        "McpServer__DefaultTenantId": "your-tenant-uuid",
        "McpServer__ServiceUserId": "your-service-user-uuid",
        "Anthropic__ApiKey": "your-key"
      }
    },
    "basefaq-direct": {
      "command": "dotnet",
      "args": ["run", "--project", "/path/to/BaseFaq.MCP.Server", "--no-build"],
      "env": {
        "ASPNETCORE_ENVIRONMENT": "Development",
        "McpServer__DefaultTenantId": "your-tenant-uuid",
        "McpServer__ServiceUserId": "your-service-user-uuid"
      }
    }
  }
}
```

The AI client loads the agent-specific prompt at session start by calling the matching MCP prompt:
- `qna_assistant` for the knowledge base agent
- `direct_assistant` for the conversation resolver
- etc.

### Claude Code (project-level)

`.claude/settings.json` at the repository root:

```json
{
  "mcpServers": {
    "basefaq": {
      "command": "dotnet",
      "args": ["run", "--project", "dotnet/BaseFaq.MCP.Server", "--no-build"],
      "env": {
        "McpServer__DefaultTenantId": "your-tenant-uuid",
        "McpServer__ServiceUserId": "your-service-user-uuid",
        "Anthropic__ApiKey": "your-key"
      }
    }
  }
}
```

### HTTP SSE transport (hosted / multi-client)

Replace `WithStdioServerTransport()` with `WithHttpTransport()` in `Program.cs` for deployments
where multiple AI clients connect to the same server instance over a network.

```csharp
builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly(...);

app.MapMcp("/mcp");
```

Client configuration then uses `type: "sse"` with the server URL instead of a local process.

---

## Cross-module tool usage

The QnA knowledge base is shared. Every agent uses `qna_search` as its first step before acting.
The cross-module handoffs mirror the BaseFAQ module boundary rules:

| Agent | Own tools | Uses from QnA | Handoff when gap found |
|---|---|---|---|
| QnA Agent | `qna_*` | — | Creates draft Q&A directly |
| Direct Agent | `direct_*` | `qna_search`, `qna_get_question` | `direct_flag_for_qna` → QnA Agent fills gap |
| Broadcast Agent | `broadcast_*` | `qna_search`, `qna_get_question` | `broadcast_flag_for_qna` → QnA Agent fills gap |
| Trust Agent | `trust_*` | `qna_search`, `qna_get_question` | `trust_publish_decision` → creates canonical QnA answer |
| Tenant Agent | `tenant_*` | optional `qna_search` for help content | n/a |

This matches the documented module handoff model exactly: Direct and Broadcast consume QnA for
answers and register gaps back to QnA. Trust publishes decisions to QnA.

---

## Environment variables

| Variable | Required | Description |
|---|---|---|
| `McpServer__DefaultTenantId` | for single-tenant | Pre-configured tenant UUID |
| `McpServer__ServiceUserId` | yes | Service account user UUID for write operations |
| `McpServer__ServiceUserName` | no | Display name for audit records |
| `McpServer__ModelName` | no | AI model for generation tools (default: `claude-sonnet-4-6`) |
| `Anthropic__ApiKey` | for generation tools | Anthropic API key |
| `ConnectionStrings__TenantDb` | yes | Shared with the rest of the backend stack |

---

## Tool availability matrix

| Tool | QnA Agent | Direct Agent | Broadcast Agent | Trust Agent | Tenant Agent | Available today |
|---|:---:|:---:|:---:|:---:|:---:|:---:|
| `qna_list_spaces` | ✅ | ✅ | ✅ | ✅ | — | ✅ |
| `qna_get_space` | ✅ | ✅ | ✅ | ✅ | — | ✅ |
| `qna_list_questions` | ✅ | ✅ | ✅ | ✅ | — | ✅ |
| `qna_get_question` | ✅ | ✅ | ✅ | ✅ | — | ✅ |
| `qna_search` | ✅ | ✅ | ✅ | ✅ | — | ❌ Gap 3 |
| `qna_create_question` | ✅ | — | — | — | — | ✅ |
| `qna_create_answer` | ✅ | — | — | — | — | ✅ |
| `qna_activate_answer` | ✅ | — | — | — | — | ✅ |
| `qna_create_source` | ✅ | — | — | — | — | ✅ |
| `qna_link_source` | ✅ | — | — | — | — | ✅ |
| `qna_import_source` | ✅ | — | — | — | — | ❌ Gap 4 |
| `direct_*` | — | ✅ | — | — | — | ❌ needs Direct API |
| `broadcast_*` | — | — | ✅ | — | — | ❌ needs Broadcast API |
| `trust_*` | — | — | — | ✅ | — | ❌ needs Trust module |
| `tenant_*` | — | — | — | — | ✅ | ✅ (via Portal API) |

---

## Gaps

### Gap 1: `AnswerKind.AiGenerated` missing

`AnswerKind` has `Official`, `Community`, `Imported`. AI-generated drafts use `Imported` today.

Fix: add `AiGenerated = 4` to `dotnet/BaseFaq.Models.QnA/Enums/AnswerKind.cs`.

### Gap 2: No `ChannelKind` for AI ingestion

`ChannelKind` values today: `Manual`, `Widget`, `Api`, `HelpCenter`, `Import`, `Other`. For
AI-generated content, use `ChannelKind.Import` (5) until a dedicated value is added.

Fix: add `AiIngestion` or `System` to `dotnet/BaseFaq.Models.QnA/Enums/ChannelKind.cs`.

### Gap 3: No full-text search in QnA Public API

`qna_search` has no backing endpoint. Every agent needs this before acting.

Fix: add `BaseFaq.QnA.Public.Business.Search` with `GET /api/qna/search?q=...`.

### Gap 4: No atomic batch creation for Source → Q&A pipeline

`qna_import_source` requires multiple sequential handler calls. A failure mid-run leaves partial
data.

Fix: add `GenerateQnAFromSourceCommand` in `BaseFaq.QnA.Portal.Business.SourceIngestion` — all
Q&A pairs and source links created in a single transaction.

For the full Source → Q&A pipeline design, gaps, and roadmap, see
[`mcp-source-to-qna.md`](mcp-source-to-qna.md).

### Gap 5: Direct, Broadcast, and Trust have no API surface

`direct_*`, `broadcast_*`, and `trust_*` tools cannot be implemented until the respective modules
ship their API hosts and business feature projects. The persistence boundaries exist; the handlers
do not.

### Gap 6: No MCP entitlement in Tenant

No per-tenant control over which modules are MCP-accessible or how many AI generation calls are
allowed per month.

Fix: add `McpEnabled` and `AiGenerationQuotaPerMonth` to `TenantEntitlementSnapshot`.

---

## Roadmap

### Phase 1 — TypeScript proxy, QnA read-only (today)

Use the TypeScript proxy described in
[`../../integrations/mcp-server.md`](../../integrations/mcp-server.md) to prove the MCP
connection with existing QnA read tools. No backend changes.

### Phase 2 — Native .NET server, QnA Agent

Create `BaseFaq.MCP.Server`. Implement QnA tools (list, get, create, link) and Tenant tools.
Implement `McpSessionService`. Ship QnA Agent and Tenant Agent configurations.

### Phase 3 — QnA Agent: search and import (close Gaps 1, 2, 3, 4)

Add `AnswerKind.AiGenerated`, `ChannelKind.AiIngestion`, `BaseFaq.QnA.Public.Business.Search`,
and `GenerateQnAFromSourceCommand`. Ship `qna_search` and `qna_import_source` tools.

### Phase 4 — Direct Agent (close Gap 5 for Direct)

Ship `BaseFaq.Direct.Portal.*` API surface. Implement `direct_*` tools and `direct_assistant`
prompt.

### Phase 5 — Broadcast Agent (close Gap 5 for Broadcast)

Ship `BaseFaq.Broadcast.Portal.*` API surface. Implement `broadcast_*` tools and
`broadcast_assistant` prompt.

### Phase 6 — Trust Agent + entitlement (close Gaps 5 for Trust, 6)

Ship Trust module. Implement `trust_*` tools and `trust_assistant` prompt. Add MCP entitlement
to Tenant.

---

## Related documents

| Document | Relationship |
|---|---|
| [`../../integrations/mcp-server.md`](../../integrations/mcp-server.md) | TypeScript proxy — quick start before Phase 2 |
| [`mcp-source-to-qna.md`](mcp-source-to-qna.md) | Deep-dive on `qna_import_source`: AI generation pipeline, Anthropic SDK integration, and Gaps 1–4 |
| [`../../business/value_proposition.md`](../../business/value_proposition.md) | Module boundaries and cross-module handoff model |
| [`../../backend/architecture/solution-architecture.md`](../../backend/architecture/solution-architecture.md) | Runtime surfaces, `ISessionService`, multitenancy model |
| [`../../backend/architecture/dotnet-backend-overview.md`](../../backend/architecture/dotnet-backend-overview.md) | Feature-scoped module pattern for adding `BaseFaq.MCP.Server` |
| [`../../backend/architecture/solution-cqrs-write-rules.md`](../../backend/architecture/solution-cqrs-write-rules.md) | CQRS write rules applied by every tool that calls a command |
| [`../../behavior-change-playbook.md`](../../behavior-change-playbook.md) | Propagation path for Gap 1 (`AnswerKind`) and Gap 2 (`ChannelKind`) |
| [`../../backend/tools/local-development.md`](../../backend/tools/local-development.md) | Starting the local stack the MCP server depends on |
