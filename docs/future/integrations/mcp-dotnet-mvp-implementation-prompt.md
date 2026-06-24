# Native .NET MCP MVP Implementation Prompt

## Purpose

Use this prompt when assigning the native `Querify.Mcp.Server` MVP to a backend and frontend
implementation agent. It is intentionally strict: if the full scope becomes too large, preserve
quality, architecture boundaries, tenant safety, and validation over shipping every tool at once.

This prompt is written for a specialist in MCP, .NET 10, ASP.NET Core, MediatR, CQRS, EF Core,
and the Querify Portal frontend.

New MCP projects must use the `Querify.Mcp.*` project and namespace prefix. Keep the product
acronym as "MCP" in prose, routes, UI copy, and tool names, but do not create new
`Querify.MCP.*` project names.

## Specialist Prompt

```text
You are a senior MCP and .NET 10 specialist working inside the Querify repository.

Goal:
Implement the native Querify MCP MVP and the Portal MCP workspace area without introducing
parallel architecture patterns. The MCP server must expose Querify behavior through module-owned
CQRS commands and queries. It must not duplicate business logic, bypass tenant resolution, or add
cross-module shortcuts.

Quality rule:
If the scope becomes too large, stage the work and finish the highest-quality slice first. Do not
scatter incomplete Direct, Broadcast, Trust, import, search, entitlement, or Portal flows across
the repo. Each completed stage must build, document what works, document what remains, and leave
clear validation notes.

Language rule:
All repository artifacts must be in English: code identifiers, comments, tests, seed data labels,
Markdown, UI copy keys, and handoff notes.

Required reading before editing:
1. docs/execution-guide.md
2. docs/behavior-change-playbook.md
3. docs/backend/architecture/solution-architecture.md
4. docs/backend/architecture/dotnet-backend-overview.md
5. docs/backend/architecture/repository-rules.md
6. docs/backend/architecture/solution-cqrs-write-rules.md
7. docs/backend/testing/integration-testing-strategy.md
8. docs/frontend/architecture/portal-app.md
9. docs/frontend/architecture/portal-app-ui-prompt-guidance.md
10. docs/frontend/architecture/portal-localization.md
11. docs/frontend/testing/validation-guide.md
12. docs/integrations/mcp-server.md
13. docs/future/integrations/mcp.md
14. docs/future/integrations/mcp-source-to-qna.md
15. Current external references:
    - https://modelcontextprotocol.io/specification/2025-06-18
    - https://developers.openai.com/apps-sdk/concepts/mcp-server
    - https://developers.openai.com/apps-sdk/mcp-apps-in-chatgpt
    - https://developers.openai.com/api/docs/guides/tools-connectors-mcp
    - https://developers.openai.com/api/docs/guides/agents
    - https://developers.openai.com/api/docs/actions/introduction

Non-negotiable implementation rules:
- Use .NET 10 only. Do not add projects targeting another framework.
- New MCP-owned projects must use the `Querify.Mcp.*` prefix. Do not create `Querify.MCP.*`
  projects.
- Use the official C# MCP SDK package already aligned with the repo restore assets when available.
- Create only the backend projects needed for the current quality stage.
- Keep runtime hosts as composition roots.
- Keep all module business behavior in the owning feature project.
- MCP tools are adapters. They set MCP session context, call MediatR commands or queries, and
  serialize tool results. They do not contain business decisions.
- Read tools must call query handlers.
- Write tools must call command handlers.
- Command handlers must return only simple values: Guid, bool, string, or void. Do not introduce
  complex command result DTOs. If a rich post-write result is needed, return a simple write
  outcome from the command and expose a separate query tool for reading the created state.
- Do not run or generate EF migrations unless explicitly asked. Leave manual migration notes for
  any schema change.
- Preserve tenant isolation. Portal-like MCP tools must resolve tenant context through the same
  ISessionService contract the existing handlers use.
- Throw ApiErrorException for user-correctable API/tool input failures inside backend behavior.
- Do not expose Direct, Broadcast, or Trust tools as callable tools until their module API/business
  command and query surfaces exist. Document them as future behavior instead.
- Portal UI copy is frontend-owned and must be localized in every locale file when UI is added.

Stage 0: Inventory and scope decision
1. Search for existing MCP, QnA, Tenant, Source, Direct, Broadcast, and Trust behavior in dotnet,
   apps, and docs.
2. Record which existing commands and queries can back MCP tools today.
3. Record gaps that require new feature projects or contracts.
4. Identify every project that will be added, which solution folder it belongs to, and whether it
   is MCP-owned, QnA-owned, Tenant-owned, or future-module-owned.
5. Split the work if completing all MCP tools, QnA search, Source Generation, Direct, Broadcast,
   Trust, entitlements, and Portal UX in one pass would reduce quality.

Current relevant solution projects:
- QnA model and domain: `Querify.Models.QnA`, `Querify.QnA.Common.Domain`.
- QnA persistence: `Querify.QnA.Common.Persistence.QnADb`.
- QnA Portal host: `Querify.QnA.Portal.Api`.
- QnA Portal feature projects: `Querify.QnA.Portal.Business.Space`,
  `Querify.QnA.Portal.Business.Question`, `Querify.QnA.Portal.Business.Answer`,
  `Querify.QnA.Portal.Business.Source`, `Querify.QnA.Portal.Business.Tag`,
  `Querify.QnA.Portal.Business.Activity`.
- QnA Worker host and source worker feature:
  `Querify.QnA.Worker.Api`, `Querify.QnA.Worker.Business.Source`.
- QnA test projects: `Querify.QnA.Portal.Test.IntegrationTests`,
  `Querify.QnA.Worker.Test.IntegrationTests`, `Querify.QnA.Public.Test.IntegrationTests`.
- Tenant Portal projects used by Stage 1 MCP read tools:
  `Querify.Tenant.Portal.Business.Tenant`, `Querify.Tenant.Portal.Business.User`,
  `Querify.Tenant.Portal.Business.Billing`.
- Direct, Broadcast, and Trust persistence boundaries exist, but their business/API surfaces are
  not available for MCP tools yet.

New MCP-owned solution folder layout:
```text
Mcp
  Common
    Querify.Mcp.Common
  Server
    Querify.Mcp.Server
  Portal
    Api
      Querify.Mcp.Portal.Api
    Business
      Querify.Mcp.Portal.Business.Configuration
  Test
    Querify.Mcp.Server.Test.IntegrationTests
    Querify.Mcp.Portal.Test.IntegrationTests
```

MCP project ownership:
- `Querify.Mcp.Common` owns MCP constants, tool names, prompt names, shared result serialization,
  and tool authorization gates. It must not contain QnA, Tenant, Direct, Broadcast, or Trust
  business behavior.
- `Querify.Mcp.Server` owns MCP transport, tool registration, prompt registration, MCP session
  context, and adapter-only tool methods.
- `Querify.Mcp.Portal.Api` is only needed when the Portal requires live MCP status, connection
  snippets, or hosted transport management. It is an authenticated Portal management API, not the
  stdio MCP server.
- `Querify.Mcp.Portal.Business.Configuration` owns read-side Portal status/configuration queries
  for MCP setup. It must not mutate QnA content.
- `Querify.Mcp.Server.Test.IntegrationTests` covers tool registration, prompt registration,
  tenant-context guards, write-tool gating, and MediatR dispatch smoke tests.
- `Querify.Mcp.Portal.Test.IntegrationTests` is only needed when `Querify.Mcp.Portal.Api` is added.

New QnA-owned Source Generation solution folder layout:
```text
QnA
  Portal
    Business
      Querify.QnA.Portal.Business.SourceGeneration
  Worker
    Business
      Querify.QnA.Worker.Business.SourceGeneration
  Test
    Querify.QnA.Portal.Test.IntegrationTests
    Querify.QnA.Worker.Test.IntegrationTests
```

Source Generation ownership:
- Source-to-Space generation writes QnA state, so it belongs to QnA, not to `Querify.Mcp.*`.
- Add request/response DTOs under `Querify.Models.QnA/Dtos/SourceGeneration`.
- Add persisted run state under `Querify.QnA.Common.Domain` and
  `Querify.QnA.Common.Persistence.QnADb` only if asynchronous generation status is required.
- Add the Portal API endpoint through `Querify.QnA.Portal.Business.SourceGeneration`, then register
  that feature in `Querify.QnA.Portal.Api`.
- Add worker execution through `Querify.QnA.Worker.Business.SourceGeneration`, then register that
  feature in `Querify.QnA.Worker.Api`.
- Add tests in feature folders such as
  `Tests/SourceGeneration/SourceGenerationCommandQueryTests.cs`.

Expected stage decision:
- Stage 1 ships native MCP foundation, QnA read/write tools backed by existing handlers, Tenant
  read tools backed by existing handlers, MCP prompts, docs, and a Portal MCP workspace area.
- Stage 2 adds QnA search after its feature-scoped read surface is implemented cleanly.
- Stage 3 adds Source Detail "Generate Space from Source" through QnA-owned Source Generation
  projects. MCP exposes it only as an adapter tool after the Portal command/query surface exists.
- Later stages add Direct, Broadcast, Trust, and MCP entitlements only when those owning module
  surfaces exist.

Stage 1 backend: Native MCP server foundation
Create:
- dotnet/Querify.Mcp.Common/Querify.Mcp.Common.csproj
- dotnet/Querify.Mcp.Server/Querify.Mcp.Server.csproj
- dotnet/Querify.Mcp.Server/Program.cs
- dotnet/Querify.Mcp.Server/Options/McpServerOptions.cs
- dotnet/Querify.Mcp.Server/Infrastructure/McpSessionContext.cs
- dotnet/Querify.Mcp.Server/Infrastructure/McpSessionService.cs
- dotnet/Querify.Mcp.Server/Tools/QnATools.cs
- dotnet/Querify.Mcp.Server/Tools/TenantTools.cs
- dotnet/Querify.Mcp.Server/Prompts/AgentPrompts.cs
- dotnet/Querify.Mcp.Server/Serialization/McpJson.cs or an equivalent small local serializer
- dotnet/Querify.Mcp.Server/Extensions/ServiceCollectionExtensions.cs if composition needs to be
  kept tidy
- dotnet/Querify.Mcp.Server.Test.IntegrationTests/Querify.Mcp.Server.Test.IntegrationTests.csproj

Project rules:
- Target net10.0.
- Reference ModelContextProtocol 1.3.0 or the currently restored official package version.
- Reference only existing Querify projects needed for the tools in this stage.
- Add the projects to `Querify.sln` under the `Mcp` solution folder tree described above.
- Register existing QnA Portal business features through their existing Add*Business extension
  methods.
- Register existing Tenant Portal business features through their existing Add*Business extension
  methods.
- Register TenantDb and QnADb exactly through existing infrastructure extension methods.
- Register ISessionService as McpSessionService in MCP scope.
- Register IHttpContextAccessor if any existing handler requires it.
- Use WithStdioServerTransport for local/client-launched MVP.
- Document HTTP/SSE as a later hosting mode unless ModelContextProtocol.AspNetCore is explicitly
  added and validated.

McpSessionContext requirements:
- TenantId: Guid
- UserId: Guid
- UserName: string?
- IsConfigured: bool or equivalent guard
- A Set method that validates tenant and user ids are non-empty.

McpSessionService requirements:
- Implement the existing ISessionService contract exactly.
- GetTenantId(ModuleEnum module) returns the scoped MCP tenant id.
- GetUserId() returns the scoped MCP service user id.
- GetUserName() returns the scoped MCP service user name when the contract supports it.
- Throw a clear ApiErrorException or InvalidOperationException for missing MCP context depending
  on whether the failure is user-correctable tool input or server configuration.

McpServerOptions requirements:
- SectionName = "McpServer"
- DefaultTenantId: Guid?
- ServiceUserId: Guid
- ServiceUserName: string?
- EnableWriteTools: bool, default false unless the repo's configuration pattern supports true
- ToolResultMaxItems: int, default bounded value
- IncludeInternalIds: bool, default true for authenticated operator tools

Tenant context rule:
- Every tool accepts tenantId when multi-tenant context is required.
- If tenantId is omitted and DefaultTenantId exists, use DefaultTenantId.
- If neither exists, reject the tool call with a clear error.
- Set McpSessionContext before dispatching MediatR.

Stage 1 backend: QnA tools
Implement only tools backed by existing QnA commands and queries.

Read tools:
- qna_list_spaces -> SpacesGetSpaceListQuery
- qna_get_space -> SpacesGetSpaceQuery or the existing single-space query
- qna_list_questions -> QuestionsGetQuestionListQuery
- qna_get_question -> QuestionsGetQuestionQuery
- qna_list_sources -> SourcesGetSourceListQuery
- qna_get_source -> SourcesGetSourceQuery

Write tools:
- qna_create_question -> QuestionsCreateQuestionCommand
- qna_create_answer -> AnswersCreateAnswerCommand
- qna_activate_answer -> AnswersActivateAnswerCommand
- qna_create_source -> SourcesCreateSourceCommand
- qna_link_question_source -> QuestionsAddSourceCommand
- qna_link_answer_source -> AnswersAddSourceCommand

Tool behavior rules:
- Write tools must require EnableWriteTools or an equivalent explicit server option.
- Created questions and answers must default to Draft and Internal unless the tool parameter
  explicitly allows another supported value and the owning command permits it.
- Do not invent an "AI published" path. Human review remains required.
- qna_create_answer may accept followUpQuestionIds only if the existing Answer create request
  supports it.
- qna_link_*_source tools must use SourceRole and Visibility according to existing DTOs and enums.
- Return serialized query DTOs for read tools.
- Return serialized simple write outcomes for write tools, such as { "id": "..." }.

Do not implement in Stage 1:
- qna_search unless a real QnA search query/endpoint is added.
- qna_generate_space_from_source unless the QnA-owned SourceGeneration command and query surface
  exists and respects simple command return rules.
- Direct, Broadcast, or Trust tools.

Stage 1 backend: Tenant tools
Implement only tools backed by existing Tenant Portal handlers.

Read tools:
- tenant_list_workspaces -> TenantsGetAllTenantsQuery
- tenant_get_client_key -> TenantsGetClientKeyQuery
- tenant_list_members -> TenantUsersGetTenantUserListQuery
- tenant_get_profile -> UsersGetUserProfileQuery
- tenant_get_billing_summary -> GetBillingSummaryQuery
- tenant_get_subscription -> GetBillingSubscriptionQuery

Tenant tool rules:
- Keep tenant tools read-only in Stage 1.
- Do not add permission, member, billing, or client-key mutation tools in Stage 1.
- Never generate or rotate client keys from MCP without a later explicit confirmation design.

Research-backed integration strategy:
- Use MCP as the primary tool contract. MCP servers expose tools that models call with structured
  parameters, and MCP also supports server-side prompts and resources. Model each Querify action as
  a narrow tool backed by one CQRS command or query.
- For local operators, IDE agents, Claude Desktop, Cursor, VS Code, and Codex-like tools, ship
  `Querify.Mcp.Server` with stdio transport first.
- For ChatGPT Apps and hosted multi-client use, add hosted MCP transport later. Prefer Streamable
  HTTP for hosted ChatGPT/App usage when the selected C# SDK package supports it cleanly.
- For ChatGPT UI surfaces, build with MCP Apps standard metadata and bridge behavior first. Use
  ChatGPT-specific `window.openai` extensions only for capabilities that are not available through
  the shared MCP Apps standard.
- For OpenAI API agent workflows, prefer the OpenAI Agents SDK or Responses API with the remote MCP
  connector when Querify tools need to participate in multi-agent orchestration, handoffs,
  tracing, or guardrails.
- For Custom GPTs, use GPT Actions only as a REST/OpenAPI facade. Custom GPT Actions call REST
  endpoints through schema-driven API calls; they are not the same as stdio MCP. Any REST facade
  must call the same CQRS services as MCP tools and must not duplicate business logic.
- Sensitive writes must require explicit approval or an equivalent confirmation flow. Treat source
  generation, content creation, activation, key rotation, permission changes, billing changes, and
  public posting as sensitive actions.
- Tool descriptions must say whether a tool reads, creates Draft/Internal content, starts an async
  run, or publishes/activates content. Do not rely on hidden prompt text to enforce write safety.
- Use MCP resources for static setup material, schemas, and connection snippets. Use MCP prompts
  for agent roles and workflows. Use MCP tools only for executable reads and actions.
- Keep `Querify.Mcp.Server` as the source of truth for tool names and semantics. If a Custom GPT
  Action or Portal API exposes the same capability, it must map to the same command/query boundary.

Stage 1 backend: Prompts
Create MCP prompts:
- qna_assistant
- tenant_assistant

Prompt behavior:
- QnA assistant checks existing QnA before suggesting new content.
- QnA assistant creates Draft/Internal content only.
- Tenant assistant is read-only unless a later stage adds confirmed write tools.
- Include Direct, Broadcast, and Trust prompt text in documentation only until callable tools exist.

Stage 1 backend validation:
- dotnet build dotnet/Querify.Mcp.Server/Querify.Mcp.Server.csproj -v minimal
- dotnet build Querify.sln -v minimal when project references or solution membership changed
- Do not finish with unbuildable generated projects.

Stage 2 backend: QnA search
Create a feature-scoped project only if needed:
- dotnet/Querify.QnA.Public.Business.Search or the smallest existing owner if the repo already
  has a search feature by the time this is implemented.

Search rules:
- Implement qna_search as a read path.
- Use AsNoTracking and direct DTO projection.
- Add filters for tenant, visibility, status, space, and search text.
- Add or update indexes when new high-cardinality lookup or text filters are introduced.
- Do not use QnA search as a generic Direct/Broadcast/Trust persistence shortcut.
- Expose MCP qna_search only after query tests exist.

Stage 3 backend: Source Detail "Generate Space from Source"
Create only after Stage 2 is stable:
- dotnet/Querify.QnA.Portal.Business.SourceGeneration/Querify.QnA.Portal.Business.SourceGeneration.csproj
- dotnet/Querify.QnA.Worker.Business.SourceGeneration/Querify.QnA.Worker.Business.SourceGeneration.csproj
- dotnet/Querify.Models.QnA/Dtos/SourceGeneration
- optional QnA run entities under `Querify.QnA.Common.Domain/Entities` and EF configuration under
  `Querify.QnA.Common.Persistence.QnADb` when durable run status is required

Source Generation is QnA-owned:
- Do not implement this as `Querify.Mcp.*` business behavior.
- Do not add the generation command to `Querify.QnA.Portal.Business.Source`; create the
  feature-scoped `SourceGeneration` business project.
- Register `AddSourceGenerationBusiness()` in `Querify.QnA.Portal.Api`.
- Register the worker execution feature in `Querify.QnA.Worker.Api` when async processing is used.
- Add the new projects to `Querify.sln` under `QnA/Portal/Business` and `QnA/Worker/Business`.

API shape:
- `POST /api/qna/source/{sourceId:guid}/generate-space`
  starts generation and returns `202 Accepted` with a correlation `Guid`.
- `GET /api/qna/source-generation/{runId:guid}`
  returns the run status, request options, failure reason, and created space id when complete.
- `GET /api/qna/source/{sourceId:guid}/generation-runs`
  lists recent runs for the Source detail page.

CQRS shape:
- `SourcesCreateSpaceGenerationRunCommand : IRequest<Guid>` returns the generation run id only.
- `SourcesExecuteSpaceGenerationRunCommand : IRequest<Guid>` is called by the worker and returns
  the created space id or the completed run id as a simple `Guid`.
- `SourcesGetSpaceGenerationRunQuery` returns the rich run DTO.
- `SourcesGetSpaceGenerationRunListQuery` returns source-scoped run summaries.
- Do not implement a command returning a complex `GenerateQnAResult` or graph DTO.
- Do not do read-after-write inside a command just to shape a response.

Execution model:
- Prefer async execution for quality. Source content fetching, model calls, recursive Q&A
  generation, tag generation, and graph creation can be slow and should not block the Portal
  request.
- The Portal command validates the source, request options, tenant context, and write permission,
  persists or enqueues a run, publishes the worker trigger, then returns the correlation id.
- The worker command loads the run, fetches or extracts content, calls the generation service,
  validates the generated plan, creates the QnA graph, finalizes run status, and saves changes.
- If the first implementation is synchronous for a narrow local MVP, keep the public command return
  simple and document that async worker execution is the production target.

Generated graph rules:
- Create one new Space from the selected Source.
- Curate the Source on the generated Space through `SpaceSource` with an explicit `SourceRole`.
- Create generated Tags and attach them to the Space and relevant Questions. Reuse existing tags by
  normalized tenant/name when possible instead of creating duplicates.
- Create top-level Questions under the new Space.
- Create Answers under each Question.
- Create child follow-up Questions through `Question.ParentAnswerId` and
  `Answer.FollowUpQuestions`.
- Prevent recursive loops by validating the generated plan as a tree before writing entities.
- Link the originating Source to the generated Space, each generated Question, and each generated
  Answer with `SourceRole.Evidence`.
- Preserve tenant id on every generated entity and relationship.
- Set generated Space, Questions, Answers, and Tags to safe review defaults. Generated Space,
  Questions, and Answers must enter Draft/Internal. Do not allow the Source Detail generation
  action to choose Public visibility while the Space is Draft.
- Do not auto-activate or publicly expose generated content.

Generation plan contract:
- The model-facing generator returns a structured plan with temporary stable ids, not direct EF
  entities.
- Required plan sections: space, tags, questions, answers, follow-up question links, source links,
  confidence scores, and warnings.
- The command maps temporary ids to entity ids inside the transaction.
- Reject plans with missing parent ids, cycles, duplicate question titles in the same branch,
  unsupported statuses, unsupported visibility, empty answers, or tags that exceed configured
  limits.
- Store the raw model output only in run metadata when it is useful for audit/debug and does not
  exceed the metadata limit.

Source content rules:
- External URL fetching, uploaded file extraction, and model invocation are adapter concerns owned
  by the SourceGeneration feature or worker service layer, not persistence entities.
- The generation command owns validation, QnA graph creation, and persistence.
- The generator must ground every answer in the Source content. If the model cannot ground an
  answer, the plan should mark it as rejected or warning-only instead of writing it as an Answer.

Enum/schema rules:
- Add `AnswerKind.AiGenerated` and `ChannelKind.AiIngestion` only through the behavior-change
  playbook, with enum XML summaries, explicit numeric values, frontend enum updates, tests, and
  migration notes.
- Do not run EF migrations unless explicitly asked.

Stage 4 backend: Future modules
Direct:
- Add Direct MCP tools only after Direct Portal/Public business feature projects exist.
- Direct tools must call Direct-owned commands/queries.
- Cross-module QnA use is limited to qna_search and qna_get_question for grounding.
- Direct gaps must be Direct-owned behavior, not QnA fields.

Broadcast:
- Add Broadcast MCP tools only after Broadcast business feature projects exist.
- Broadcast public/community state belongs to Broadcast persistence.
- Broadcast may consume QnA canonical answers and flag QnA gaps through a documented handoff.

Trust:
- Add Trust MCP tools only after Trust contracts, persistence, and business projects exist.
- Trust decisions belong to Trust.
- Publishing final decisions into QnA must be an explicit handoff command/query flow.

Entitlements:
- Add MCP entitlement only in Tenant-owned contracts/entities when the product decision is ready.
- Use the behavior-change playbook.
- Leave manual migration notes unless migrations are explicitly requested.

Stage 1 documentation
Update:
- docs/integrations/mcp-server.md with native .NET MVP status and commands to run it.
- docs/future/integrations/mcp.md with the actual implemented Stage 1 status.
- docs/README.md and docs/future/README.md links if new stable docs are added.

Required documentation content:
- How to run `Querify.Mcp.Server` locally.
- Required environment variables.
- How to connect Claude Desktop, Claude Code, Cursor, VS Code, or another MCP client through stdio.
- Which tools are available now.
- Which tools are intentionally future.
- How tenant context is resolved.
- How write tools are enabled or disabled.
- How command/query ownership works.
- Validation commands used.
- Manual migration notes if any schema-affecting changes were staged but migrations were not run.

Stage 1 frontend: Portal MCP workspace area
Create a new authenticated Portal area called MCP under the Workspace navigation.

Suggested files:
- apps/portal/src/domains/mcp/routes.tsx
- apps/portal/src/domains/mcp/mcp-page.tsx
- apps/portal/src/domains/mcp/types.ts
- apps/portal/src/domains/mcp/api.ts only when a backend Portal API exists
- apps/portal/src/domains/mcp/hooks.ts only when there is real data to fetch

Navigation:
- Add MCP to the Workspace group in the Portal sidebar.
- Use a lucide icon such as Bot, Cable, Network, or PlugZap.
- Route should be stable and workspace-scoped, for example /workspace/mcp or the existing route
  convention used by the Portal shell.

MVP page purpose:
- Show the tenant operator what MCP tools are available, how they are grouped, and how to connect
  local clients.
- Show QnA and Tenant tool groups as available when Stage 1 backend is implemented.
- Show Direct, Broadcast, Trust, Source Generation, search, HTTP/SSE transport, and entitlements as
  future/blocked by module gaps.
- Keep copy concise and operational. Do not create a marketing page.

MVP page sections:
- Server status: local stdio MVP, write tools disabled/enabled, default tenant configured or not.
- Available tools: grouped table for QnA and Tenant.
- Agent prompts: QnA assistant and Tenant assistant.
- Client connection: config snippets for local stdio clients.
- Tenant context: DefaultTenantId and explicit tenantId behavior.
- Future modules: Direct, Broadcast, Trust readiness matrix.
- Validation checklist: build command, MCP inspector command, and smoke-test calls.

Frontend UI rules:
- Reuse shared Portal layout primitives.
- Prefer a task-oriented dashboard/list composition, not a landing hero.
- Use compact cards only for individual repeated items, not nested cards.
- Use status badges from the shared enum/status system where possible.
- Use ActionPanel and ActionButton for screen-level actions.
- Use tabs or segmented controls if the page grows into Overview, Tools, Prompts, Clients, and
  Runs.
- Support light and dark mode.
- Support 320 CSS pixel width with no horizontal overflow.
- Do not hardcode English text directly in components; add en-US keys and translate every other
  locale file in the same frontend change.
- Long ids, paths, URLs, config snippets, and JSON must wrap or scroll inside their own code
  block without forcing page overflow.

Frontend future API contract:
- If the Portal needs live MCP status, add Tenant Portal-owned read queries and DTOs first.
- Do not make the Portal call the MCP stdio process directly.
- A future hosted MCP status endpoint belongs to a backend HTTP surface with authentication and
  tenant authorization, not to the stdio transport.

Stage 3 frontend: Source Detail generate action
Add a Source Detail action that starts the QnA-owned Source Generation flow.

Files likely affected:
- apps/portal/src/domains/sources/source-detail-page.tsx
- apps/portal/src/domains/sources/api.ts
- apps/portal/src/domains/sources/hooks.ts
- apps/portal/src/domains/sources/types.ts
- apps/portal/src/domains/sources/source-generate-space-dialog.tsx
- apps/portal/src/shared/lib/i18n/locales/*.json

Action placement:
- Add the action to the existing Source Detail `ActionPanel layout="bar"`.
- Use a clear icon such as `Sparkles`, `FolderPlus`, or `Bot`.
- Label: "Generate space" or "Generate Q&A space".
- Treat it as a meaningful long-running action with an explicit confirmation or dialog.
- Disable it when the source id is missing, the source is deleted, an upload is not verified, or
  the source has no usable locator/storage content.
- Do not call `Querify.Mcp.Server` from the browser. The button calls the QnA Portal API endpoint.

Best form shape:
- Show an automatic planning summary. The backend command derives Space name, slug, language,
  graph size, maximum top-level questions, follow-up depth, answers per question, tag behavior,
  Evidence source links, and required citations from the selected Source and the optional manual
  guidance.
- Do not expose manual controls for Space name, slug, language, visibility, status, accepts
  questions, accepts answers, maximum top-level questions, follow-up depth, answers per question,
  include follow-ups, tag generation mode, source role, or citation requirement.
- Keep only two manual fields:
  - Extraction goal or audience note.
  - Content range or section hint for long sources.
- Place a `ContextHint` beside each manual field using the Querify information icon pattern to
  explain when the field helps the generation agent.
- Generated Space, Questions, and Answers remain Draft/Internal while under review because the
  current Space visibility rule allows Public exposure only after activation.
- Primary action starts the run.

Portal behavior:
- On submit, call `POST /api/qna/source/{sourceId}/generate-space`.
- Show a pending state on the button and dialog submit action.
- On accepted response, show the run id and keep the operator on Source Detail.
- Add or reuse a Source Detail relationship section for generation runs when the backend query
  exists.
- When the run completes and returns a created space id, show an action to open the generated Space.
- If SignalR notification support is added for generation runs, keep it as UX acceleration; the
  Source Detail page must still query authoritative status.
- Add loading, empty, error, pending, success, and failed generation states.
- Add all UI copy to every locale file in the same change.

Frontend validation:
- npm run lint
- npm run build
- Manual responsive pass at 320, 360, 375, 414, 768, 1024, 1279, 1280, and desktop widths.
- Manual light and dark mode pass.
- Locale key parity validation according to portal-localization.md.

Testing expectations:
- Add integration or architecture tests when introducing backend contracts or project rules.
- For MCP server smoke coverage, prefer tests that verify tool registration, tenant context guard,
  write-tool gating, and MediatR dispatch boundaries.
- Do not weaken production constructors or required fields to make tests easier.

Completion report:
End with a concise status report:
- Implemented stage and files changed.
- Available MCP tools.
- Portal area status.
- Validation commands run and results.
- Intentional follow-ups.
- Manual migration note: none, or exact pending operations.
```

## Implementation Staging Summary

| Stage | Outcome | Quality gate |
|---|---|---|
| 1 | Native MCP server, QnA/Tenant tools, prompts, docs, Portal MCP area | `Querify.Mcp.*` projects build, existing CQRS handlers are used, Portal builds |
| 2 | QnA search | Query tests, no-tracking projections, indexes/migration notes |
| 3 | Source Detail Generate Space from Source | QnA SourceGeneration command returns `Guid`, async run status is query-owned, generated graph is atomic |
| 4 | Direct tools | Direct-owned commands/queries exist first |
| 5 | Broadcast tools | Broadcast-owned commands/queries exist first |
| 6 | Trust tools and MCP entitlements | Trust/Tenant ownership is implemented through the playbook |

## Project Inventory

| Project | Stage | Solution folder | Ownership |
|---|---:|---|---|
| `Querify.Mcp.Common` | 1 | `Mcp/Common` | MCP constants, tool names, prompt names, serialization, and tool gates |
| `Querify.Mcp.Server` | 1 | `Mcp/Server` | MCP stdio/hosted transport, tool adapters, prompts, session context |
| `Querify.Mcp.Server.Test.IntegrationTests` | 1 | `Mcp/Test` | MCP server registration, tenant context, write gating, dispatch smoke tests |
| `Querify.Mcp.Portal.Api` | 1B/later | `Mcp/Portal/Api` | Authenticated Portal MCP status/configuration API when live status is needed |
| `Querify.Mcp.Portal.Business.Configuration` | 1B/later | `Mcp/Portal/Business` | Portal read queries for MCP setup, status, and connection snippets |
| `Querify.Mcp.Portal.Test.IntegrationTests` | 1B/later | `Mcp/Test` | Portal MCP API integration tests |
| `Querify.QnA.Public.Business.Search` | 2 | `QnA/Public/Business` | QnA-owned search query surface for MCP and public consumers |
| `Querify.QnA.Portal.Business.SourceGeneration` | 3 | `QnA/Portal/Business` | QnA-owned Source Detail generation start/status use cases |
| `Querify.QnA.Worker.Business.SourceGeneration` | 3 | `QnA/Worker/Business` | QnA-owned async execution of Source-to-Space generation runs |
| `Querify.Models.QnA/Dtos/SourceGeneration` | 3 | existing `Models` project | QnA Source Generation request/read DTOs |
| `Querify.QnA.Common.Domain` additions | 3 | `QnA/Common/Domain` | Optional generation run entities when durable run state is required |
| `Querify.QnA.Common.Persistence.QnADb` additions | 3 | `QnA/Common/Persistence` | Optional generation run EF configuration and indexes |

Do not create Direct, Broadcast, or Trust MCP tool projects until those modules have real
business/API surfaces. Do not create a `Querify.Mcp.SourceGeneration` project; generation writes
QnA content and belongs to QnA.

## MVP Tool Matrix

| Tool | Stage | Backing owner | CQRS boundary |
|---|---:|---|---|
| `qna_list_spaces` | 1 | QnA Portal Space | Query |
| `qna_get_space` | 1 | QnA Portal Space | Query |
| `qna_list_questions` | 1 | QnA Portal Question | Query |
| `qna_get_question` | 1 | QnA Portal Question | Query |
| `qna_list_sources` | 1 | QnA Portal Source | Query |
| `qna_get_source` | 1 | QnA Portal Source | Query |
| `qna_create_question` | 1 | QnA Portal Question | Command returns `Guid` |
| `qna_create_answer` | 1 | QnA Portal Answer | Command returns `Guid` |
| `qna_activate_answer` | 1 | QnA Portal Answer | Command returns simple value |
| `qna_create_source` | 1 | QnA Portal Source | Command returns `Guid` |
| `qna_link_question_source` | 1 | QnA Portal Question | Command returns simple value |
| `qna_link_answer_source` | 1 | QnA Portal Answer | Command returns simple value |
| `tenant_list_workspaces` | 1 | Tenant Portal Tenant | Query |
| `tenant_get_client_key` | 1 | Tenant Portal Tenant | Query |
| `tenant_list_members` | 1 | Tenant Portal TenantUser | Query |
| `tenant_get_profile` | 1 | Tenant Portal User | Query |
| `tenant_get_billing_summary` | 1 | Tenant Portal Billing | Query |
| `tenant_get_subscription` | 1 | Tenant Portal Billing | Query |
| `qna_search` | 2 | QnA Search | Query |
| `qna_generate_space_from_source` | 3 | QnA SourceGeneration | Command returns run `Guid`, result read through query |
| `qna_get_source_generation_run` | 3 | QnA SourceGeneration | Query |
| `direct_*` | 4 | Direct | Direct-owned commands/queries |
| `broadcast_*` | 5 | Broadcast | Broadcast-owned commands/queries |
| `trust_*` | 6 | Trust | Trust-owned commands/queries |

## Portal MCP Area Detail

The Portal MCP area is not a marketing page. It is an operator surface for configuring and
validating the MCP interface for a workspace.

Recommended first screen:
- Route: `/workspace/mcp`
- Sidebar group: Workspace
- Title: MCP
- Primary action: copy local client config
- Secondary action: open documentation
- Status rail: server mode, write tools, default tenant, service user
- Main table: tools grouped by module with availability, backing CQRS owner, and stage
- Prompt panel: available prompts and intended agent role
- Future readiness panel: Direct, Broadcast, Trust, search, Source Generation, and entitlements

Do not add live run history, quotas, hosted transport controls, or entitlement editing until the
backend has real Tenant-owned contracts for those behaviors.

## Manual Migration Note

Stage 1 should require no EF migration. If a later stage adds enum values, persisted fields,
entitlements, generation run state, or search indexes, do not run migrations by default. Leave a
manual migration note listing the intended schema operations and the owning module.
