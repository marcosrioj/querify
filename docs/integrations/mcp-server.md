# Querify MCP Server

## Status

`Querify.Mcp.Server` is the native .NET 10 MCP Stage 3 server. It runs over stdio for local
MCP clients and exposes QnA, QnA Search, Source Generation, and Tenant behavior through
module-owned MediatR CQRS handlers.

The server is intentionally not a browser API. The Portal MCP page at `/app/mcp` explains the
available tools and connection shape; it does not call the stdio process.

## Projects

| Project | Solution folder | Purpose |
|---|---|---|
| `Querify.Mcp.Common` | `Mcp/Common` | Tool names, prompt names, result serialization, write-tool gate |
| `Querify.Mcp.Server` | `Mcp/Server` | Stdio transport, MCP tool adapters, prompts, session context |
| `Querify.Mcp.Server.Test.IntegrationTests` | `Mcp/Test` | Tool/prompt metadata, tenant-context guard, write gate coverage |
| `Querify.QnA.Public.Business.Search` | `QnA/Public/Business` | QnA-owned search query surface used by `qna_search` |
| `Querify.QnA.Portal.Business.SourceGeneration` | `QnA/Portal/Business` | QnA-owned start/status API and CQRS surface for source-to-space generation |
| `Querify.QnA.Worker.Business.SourceGeneration` | `QnA/Worker/Business` | QnA-owned execution handler for generation runs |

Stage 3 does not create `Querify.Mcp.Portal.Api`, Direct/Broadcast/Trust tools, hosted MCP
transport, or Tenant-owned MCP entitlements.

## Run Locally

```bash
dotnet run --project dotnet/Querify.Mcp.Server/Querify.Mcp.Server.csproj
```

Required configuration:

```bash
ConnectionStrings__TenantDb=Host=localhost;Port=5432;Database=querify_tenant;Username=...;Password=...
McpServer__ServiceUserId=<service-user-guid>
McpServer__ServiceUserName=system:mcp
McpServer__DefaultTenantId=<tenant-guid>
McpServer__EnableWriteTools=false
McpServer__ToolResultMaxItems=20
```

`McpServer__DefaultTenantId` is optional only when every tool call passes `tenantId`.
`McpServer__EnableWriteTools` defaults to `false`; leave it disabled unless the operator session is
explicitly approved for QnA draft writes.

## Client Config

Use this shape for Claude Desktop, Claude Code, Cursor, VS Code, or another stdio MCP client:

```json
{
  "mcpServers": {
    "querify": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "/absolute/path/to/querify/dotnet/Querify.Mcp.Server/Querify.Mcp.Server.csproj"
      ],
      "env": {
        "ConnectionStrings__TenantDb": "Host=localhost;Port=5432;Database=querify_tenant;Username=...;Password=...",
        "McpServer__ServiceUserId": "service-user-guid",
        "McpServer__DefaultTenantId": "tenant-guid",
        "McpServer__EnableWriteTools": "false"
      }
    }
  }
}
```

## Tools

### QnA

| Tool | CQRS owner | Boundary |
|---|---|---|
| `qna_list_spaces` | `Querify.QnA.Portal.Business.Space` | Query |
| `qna_get_space` | `Querify.QnA.Portal.Business.Space` | Query |
| `qna_list_questions` | `Querify.QnA.Portal.Business.Question` | Query |
| `qna_get_question` | `Querify.QnA.Portal.Business.Question` | Query |
| `qna_search` | `Querify.QnA.Public.Business.Search` | Query |
| `qna_list_sources` | `Querify.QnA.Portal.Business.Source` | Query |
| `qna_get_source` | `Querify.QnA.Portal.Business.Source` | Query |
| `qna_get_source_generation_run` | `Querify.QnA.Portal.Business.SourceGeneration` | Query |
| `qna_create_question` | `Querify.QnA.Portal.Business.Question` | Command returns `Guid` |
| `qna_create_answer` | `Querify.QnA.Portal.Business.Answer` | Command returns `Guid` |
| `qna_activate_answer` | `Querify.QnA.Portal.Business.Answer` | Command returns `Guid` |
| `qna_create_source` | `Querify.QnA.Portal.Business.Source` | Command returns `Guid` |
| `qna_generate_space_from_source` | `Querify.QnA.Portal.Business.SourceGeneration` | Command returns generation run `Guid` |
| `qna_link_question_source` | `Querify.QnA.Portal.Business.Question` | Command returns `Guid` |
| `qna_link_answer_source` | `Querify.QnA.Portal.Business.Answer` | Command returns `Guid` |

QnA write tools create Draft/Internal content by default and require
`McpServer__EnableWriteTools=true`.

`qna_generate_space_from_source` starts a QnA-owned generation run and returns the run id. The
command accepts only the Source id plus optional extraction goal and content hint. The QnA
SourceGeneration command derives the Space name, slug, language, graph size, follow-up depth,
answer count, tag behavior, Evidence source links, and required answer citations. The local MVP
executes the worker command in-process after creating the run; the production target is queued
worker execution using the same command/query contracts. Generated spaces, questions, and answers
remain Draft/Internal for review because the current Space rule allows public exposure only after
activation.

### Tenant

| Tool | CQRS owner | Boundary |
|---|---|---|
| `tenant_list_workspaces` | `Querify.Tenant.Portal.Business.Tenant` | Query |
| `tenant_get_client_key` | `Querify.Tenant.Portal.Business.Tenant` | Query |
| `tenant_list_members` | `Querify.Tenant.Portal.Business.Tenant` | Query |
| `tenant_get_profile` | `Querify.Tenant.Portal.Business.User` | Query |
| `tenant_get_billing_summary` | `Querify.Tenant.Portal.Business.Billing` | Query |
| `tenant_get_subscription` | `Querify.Tenant.Portal.Business.Billing` | Query |

Tenant tools remain read-only. MCP does not generate or rotate client keys.

## Prompts

| Prompt | Role |
|---|---|
| `qna_assistant` | Checks existing QnA first and creates only Draft/Internal QnA content when writes are enabled. |
| `tenant_assistant` | Keeps workspace, member, profile, billing, subscription, and client-key assistance read-only. |

## Tenant Context

Each tool sets `McpSessionContext` before dispatching MediatR. `McpSessionService` implements the
existing `ISessionService` contract used by QnA and Tenant handlers.

Resolution order:

1. Use the tool `tenantId` argument when provided.
2. Use `McpServer__DefaultTenantId` when the tool omits `tenantId`.
3. Reject the tool call with a clear MCP error when neither exists.

QnA activity-writing handlers require request IP and user-agent values. Because stdio has no HTTP
request, the MCP adapter initializes a scoped `DefaultHttpContext` with MCP-specific loopback
request metadata before dispatch.

## Future Tools

These are intentionally not callable in Stage 3:

- `direct_*`
- `broadcast_*`
- `trust_*`
- Tenant mutation, entitlement, billing mutation, permission mutation, and key-rotation tools

Future tools must be backed by owning module CQRS commands or queries before the MCP adapter is
added.

## Validation

Current validation commands:

```bash
dotnet build dotnet/Querify.Mcp.Server/Querify.Mcp.Server.csproj -v minimal
dotnet build dotnet/Querify.Mcp.Server.Test.IntegrationTests/Querify.Mcp.Server.Test.IntegrationTests.csproj -v minimal
dotnet build dotnet/Querify.QnA.Portal.Business.SourceGeneration/Querify.QnA.Portal.Business.SourceGeneration.csproj -v minimal
dotnet test dotnet/Querify.QnA.Portal.Test.IntegrationTests/Querify.QnA.Portal.Test.IntegrationTests.csproj --filter FullyQualifiedName~SourceGeneration -v minimal
dotnet test dotnet/Querify.QnA.Public.Test.IntegrationTests/Querify.QnA.Public.Test.IntegrationTests.csproj --filter FullyQualifiedName~Search -v minimal
dotnet build Querify.sln -v minimal
```

MCP inspector example:

```bash
npx @modelcontextprotocol/inspector dotnet run --project dotnet/Querify.Mcp.Server/Querify.Mcp.Server.csproj
```

## Manual Migration Note

Stage 3 schema changes are represented by the QnA migration
`20260619055744_AddMcpCallers`. The automatic planning update changes request shape and runtime
defaults, and the current Stage 3 migration default for `SourceGenerationRuns.SourceRole` is
`SourceRole.Evidence`. No additional migration was generated for this update.

See the staged roadmap in [`../future/integrations/mcp-dotnet-mvp-implementation-prompt.md`](../future/integrations/mcp-dotnet-mvp-implementation-prompt.md).
