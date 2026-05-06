# MCP: Source → Q&A Generation Pipeline

## Purpose

Deep-dive on the `qna_import_source` tool and the `GenerateQnAFromSourceCommand` it depends on.
This document covers the AI generation logic, the Anthropic SDK integration, and the gaps specific
to this pipeline.

Read [`mcp.md`](mcp.md) first for the server architecture, session model, project structure, and
the full tool/agent matrix. Everything documented there applies here.

**Status:** designed, not yet built. See [`../README.md`](../README.md).

---

## What this pipeline does

```
Source artifact (URL, PDF, transcript, article)
  → ContentFetcher strips and normalizes content
  → QnAGenerator calls Claude API with extraction prompt
  → GenerateQnAFromSourceCommand creates all Q&A pairs in one transaction
  → All content enters as Draft, linked to the originating Source record
  → Human curator reviews AiConfidenceScore, activates or discards
```

This is the "interação → ativo reutilizável" cycle applied to existing written artifacts rather
than live conversations. It populates the knowledge base from organizational content without
manual Q&A authoring.

---

## Data model for AI-generated content

Both `Question` and `Answer` already carry `AiConfidenceScore`. The field was designed for this
use case and must be populated by the pipeline.

### Question fields used by the pipeline

| Field | Value set by pipeline |
|---|---|
| `Title` | Canonical question text from AI |
| `Summary` | Short version for list and search display |
| `ContextNote` | Optional framing from AI |
| `Status` | `Draft` — always |
| `Visibility` | `Internal` — always on creation |
| `OriginChannel` | `ChannelKind.Import` today; `ChannelKind.AiIngestion` after Gap 2 |
| `AiConfidenceScore` | AI model confidence, 0–1 |
| `SpaceId` | Target space from tool parameter |

### Answer fields used by the pipeline

| Field | Value set by pipeline |
|---|---|
| `Headline` | Short answer title from AI |
| `Body` | Full answer body from AI |
| `Kind` | `AnswerKind.Imported` today; `AnswerKind.AiGenerated` after Gap 1 |
| `Status` | `Draft` — always |
| `Visibility` | `Internal` — always on creation |
| `AuthorLabel` | `"AI-generated draft (claude-sonnet-4-6)"` |
| `AiConfidenceScore` | Same value as the parent question |
| `QuestionId` | ID of the question created in the same run |

### Source fields used by the pipeline

| Field | Value set by pipeline |
|---|---|
| `Kind` | Determined from URL or tool parameter (see table below) |
| `Locator` | The URL |
| `Label` | Page title extracted from HTML |
| `Language` | From tool parameter |
| `Checksum` | SHA-256 of the raw fetched content |
| `MetadataJson` | Generation metadata (see convention below) |
| `Visibility` | `Internal` |
| `LastVerifiedAtUtc` | Time of fetch |

### SourceKind selection

| Source type | `SourceKind` value |
|---|---|
| Blog post, help article | `Article` (1) |
| Documentation page, general web page | `WebPage` (2) |
| PDF document | `Pdf` (3) |
| Video transcript | `Video` (4) |
| README, changelog, code file | `Repository` (5) |
| Release note, product update | `ProductNote` (6) |
| Internal wiki, Notion, Confluence | `InternalNote` (7) |
| Policy, contract, terms | `GovernanceRecord` (8) |
| Audit log, decision record | `AuditRecord` (9) |

### `MetadataJson` convention

Store this in `Source.MetadataJson` at import time. No backend schema change — client convention.

```json
{
  "pipeline": "mcp-source-to-qna",
  "pipelineVersion": "1.0.0",
  "model": "claude-sonnet-4-6",
  "generatedAt": "2026-04-30T12:00:00Z",
  "inputTokens": 12500,
  "outputTokens": 800,
  "pairsGenerated": 5,
  "fetchedAt": "2026-04-30T11:59:50Z",
  "contentHash": "sha256:abc123..."
}
```

---

## Tools

### `qna_analyze_source` — preview without saving

```csharp
[McpServerTool(Name = "qna_analyze_source",
    Description = "Analyze a source URL and return Q&A candidates without saving. Use this to preview before importing.")]
public async Task<string> AnalyzeSource(
    [McpServerToolParameter(Description = "URL to analyze")] string url,
    [McpServerToolParameter(Description = "Max Q&A pairs to generate (1-20)")] int maxPairs = 5,
    CancellationToken ct = default)
{
    var (text, _, _) = await _fetcher.FetchAsync(url, ct);
    var candidates = await _generator.GenerateAsync(text, maxPairs, ct);
    return Serialize(candidates);
}
```

### `qna_import_source` — full pipeline

```csharp
[McpServerTool(Name = "qna_import_source",
    Description = "Fetch a URL, generate Q&A drafts using AI, and save everything to Querify in one transaction. All content enters as Draft and requires human review.")]
public async Task<string> ImportSource(
    [McpServerToolParameter] Guid tenantId,
    [McpServerToolParameter(Description = "URL of the source")] string url,
    [McpServerToolParameter(Description = "Target space ID")] Guid spaceId,
    [McpServerToolParameter] SourceKind sourceKind = SourceKind.WebPage,
    [McpServerToolParameter] int maxPairs = 5,
    [McpServerToolParameter] string language = "en",
    CancellationToken ct = default)
{
    SetTenantContext(tenantId);

    var (text, checksum, title) = await _fetcher.FetchAsync(url, ct);
    var candidates = await _generator.GenerateAsync(text, maxPairs, ct);

    // Single command → single transaction (Gap 4 fix)
    var result = await _mediator.Send(new GenerateQnAFromSourceCommand
    {
        SpaceId = spaceId,
        SourceLocator = url,
        SourceKind = sourceKind,
        SourceLabel = title,
        Language = language,
        Checksum = checksum,
        MetadataJson = BuildMetadata(candidates.Count, checksum),
        Candidates = candidates,
    }, ct);

    return Serialize(result);
}
```

### `qna_generate_from_existing_source`

```csharp
[McpServerTool(Name = "qna_generate_from_existing_source",
    Description = "Re-read a source already registered in Querify and generate new Q&A drafts from its current content.")]
public async Task<string> GenerateFromExistingSource(
    [McpServerToolParameter] Guid tenantId,
    [McpServerToolParameter(Description = "Querify source ID")] Guid sourceId,
    [McpServerToolParameter(Description = "Target space ID")] Guid spaceId,
    [McpServerToolParameter] int maxPairs = 5,
    CancellationToken ct = default)
{
    SetTenantContext(tenantId);

    var source = await _mediator.Send(new GetSourceQuery { SourceId = sourceId }, ct);
    var (text, checksum, _) = await _fetcher.FetchAsync(source.Locator, ct);
    var candidates = await _generator.GenerateAsync(text, maxPairs, ct);

    var result = await _mediator.Send(new GenerateQnAFromSourceCommand
    {
        SpaceId = spaceId,
        ExistingSourceId = sourceId,   // skip source creation, link to existing
        Checksum = checksum,
        MetadataJson = BuildMetadata(candidates.Count, checksum),
        Candidates = candidates,
    }, ct);

    return Serialize(result);
}
```

---

## `GenerateQnAFromSourceCommand` (Gap 4)

This command does not exist yet. It is the core backend addition for this pipeline.

```csharp
// dotnet/Querify.QnA.Portal.Business.SourceIngestion/Commands/GenerateQnA/GenerateQnAFromSourceCommand.cs
public record GenerateQnAFromSourceCommand : IRequest<GenerateQnAResult>
{
    public required Guid SpaceId { get; init; }
    public Guid? ExistingSourceId { get; init; }    // null = create new source
    public string? SourceLocator { get; init; }
    public SourceKind SourceKind { get; init; }
    public string? SourceLabel { get; init; }
    public string Language { get; init; } = "en";
    public required string Checksum { get; init; }
    public string? MetadataJson { get; init; }
    public required List<QnACandidate> Candidates { get; init; }
}

public record GenerateQnAResult
{
    public Guid SourceId { get; init; }
    public List<CreatedPair> Pairs { get; init; } = [];
}

public record CreatedPair(Guid QuestionId, Guid AnswerId, string Question);
```

The handler creates source (if new), questions, answers, and links all in one `SaveChanges()`:

```csharp
public async Task<GenerateQnAResult> Handle(
    GenerateQnAFromSourceCommand cmd, CancellationToken ct)
{
    var sourceId = cmd.ExistingSourceId ?? await CreateSourceAsync(cmd, ct);
    var pairs = new List<CreatedPair>();

    foreach (var c in cmd.Candidates)
    {
        var question = BuildQuestion(cmd.SpaceId, c);
        var answer = BuildAnswer(c);
        var qSourceLink = new QuestionSourceLink { Role = SourceRole.Origin, SourceId = sourceId };
        var aSourceLink = new AnswerSourceLink { Role = SourceRole.Origin, SourceId = sourceId };

        question.Sources.Add(qSourceLink);
        answer.Sources.Add(aSourceLink);
        question.Answers.Add(answer);

        _db.Questions.Add(question);
        pairs.Add(new(question.Id, answer.Id, question.Title));
    }

    await _db.SaveChangesAsync(ct);   // single transaction for all pairs
    return new GenerateQnAResult { SourceId = sourceId, Pairs = pairs };
}
```

Project location follows the feature-scoped module pattern:

```
dotnet/Querify.QnA.Portal.Business.SourceIngestion/
  Commands/
    GenerateQnA/
      GenerateQnAFromSourceCommand.cs
      GenerateQnAFromSourceCommandHandler.cs
  Extensions/
    ServiceCollectionExtensions.cs    AddSourceIngestionBusiness()
```

Register in `Querify.QnA.Portal.Api` alongside the existing feature modules. Also register in
`Querify.MCP.Server/Program.cs` since the MCP server calls it directly.

---

## `ContentFetcher`

```csharp
// dotnet/Querify.MCP.Server/Infrastructure/ContentFetcher.cs
public sealed class ContentFetcher(HttpClient http)
{
    public async Task<(string Text, string Checksum, string Title)> FetchAsync(
        string url, CancellationToken ct)
    {
        var response = await http.GetStringAsync(url, ct);
        var checksum = ComputeSha256(response);

        var doc = new HtmlDocument();
        doc.LoadHtml(response);

        // Remove noise nodes
        foreach (var node in doc.DocumentNode
            .SelectNodes("//nav|//footer|//script|//style|//header|//aside") ?? [])
            node.Remove();

        var title = doc.DocumentNode
            .SelectSingleNode("//title")?.InnerText.Trim() ?? url;

        var text = doc.DocumentNode.InnerText
            .Replace("\t", " ")
            .Replace("\r", "")
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim())
            .Where(l => l.Length > 0)
            .Aggregate(new StringBuilder(), (sb, l) => sb.AppendLine(l))
            .ToString();

        return (text, checksum, title);
    }

    private static string ComputeSha256(string content)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(content));
        return $"sha256:{Convert.ToHexString(bytes).ToLowerInvariant()}";
    }
}
```

---

## `QnAGenerator`

```csharp
// dotnet/Querify.MCP.Server/Services/QnAGenerator.cs
public sealed class QnAGenerator(AnthropicClient anthropic, IOptions<McpServerOptions> options)
{
    public async Task<List<QnACandidate>> GenerateAsync(
        string text, int maxPairs, CancellationToken ct)
    {
        var response = await anthropic.Messages.CreateAsync(new MessageCreateParams
        {
            Model = options.Value.ModelName,
            MaxTokens = 4096,
            System = string.Join(" ",
                "You are a knowledge base editor.",
                "Extract the most valuable question-and-answer pairs from the provided content.",
                "Each question must be a complete, standalone sentence someone would actually search for.",
                "Each answer must be accurate, concise, and grounded only in the provided content.",
                "Return a JSON array only. No explanatory text."),
            Messages =
            [
                new()
                {
                    Role = "user",
                    Content = $"""
                        Extract up to {maxPairs} Q&A pairs from the content below.
                        Return JSON array:
                        [{{"question":"...","summary":"...","answerHeadline":"...","answerBody":"...","confidence":0.0}}]
                        confidence is 0–1: how well the content supports the answer.
                        ---
                        {text[..Math.Min(text.Length, 80_000)]}
                        """,
                },
            ],
        }, ct);

        return JsonSerializer.Deserialize<List<QnACandidate>>(
            response.Content[0].Text, _jsonOptions)!;
    }
}

public record QnACandidate(
    string Question,
    string Summary,
    string AnswerHeadline,
    string AnswerBody,
    decimal Confidence);
```

---

## Gaps specific to this pipeline

These supplement the gaps in [`mcp.md`](mcp.md). Gaps 1–4 from `mcp.md` directly affect this
pipeline and are repeated here for completeness.

### Gap 1: `AnswerKind.AiGenerated` missing

**Workaround today:** `AnswerKind.Imported`.
**Fix:** add `AiGenerated = 4` to `dotnet/Querify.Models.QnA/Enums/AnswerKind.cs`.

### Gap 2: No `ChannelKind` for AI ingestion

**Workaround today:** `ChannelKind.Import` (5).
**Fix:** add `AiIngestion` to `dotnet/Querify.Models.QnA/Enums/ChannelKind.cs`.

### Gap 3: No search — duplicates not detected before generation

Without `qna_search`, the pipeline may generate questions that already exist in the knowledge
base. Full deduplication requires Gap 3 to be closed.

**Partial workaround:** before generating, call `qna_list_questions` with a text filter and
compare titles manually. Not robust, but better than nothing.

### Gap 4: `GenerateQnAFromSourceCommand` does not exist

Without this command, the pipeline makes N sequential handler calls with no transactional
guarantee. A crash midway leaves orphaned drafts.

**Workaround today (Phase 1 TypeScript proxy):** accept partial failures during prototyping.
**Fix:** implement the command as described above before shipping production workloads.

### Gap 5: Sources requiring authentication cannot be fetched

`ContentFetcher` uses `HttpClient` without credentials. Internal wikis, Notion, and Confluence
behind SSO require a dedicated connector.

**Workaround:** accept `text` as a direct parameter alongside `url`, allowing the user to
paste pre-extracted content.

---

## Phase roadmap for this pipeline

Phases 1–3 are inherited from [`mcp.md`](mcp.md). These phases are specific to this pipeline:

| Phase | What ships |
|---|---|
| Phase 1 | TypeScript proxy with `qna_analyze_source` and `qna_import_source` (sequential HTTP calls, no transaction) |
| Phase 2 | Native .NET: `ContentFetcher`, `QnAGenerator`, `qna_analyze_source`, `qna_import_source`, `qna_generate_from_existing_source` using sequential commands (Gap 4 workaround) |
| Phase 3 | `GenerateQnAFromSourceCommand` (atomic), `AnswerKind.AiGenerated`, `ChannelKind.AiIngestion`, `qna_search` for deduplication |
| Later | Authentication connectors for internal sources (Gap 5) |

---

## Related documents

| Document | Relationship |
|---|---|
| [`mcp.md`](mcp.md) | Server architecture, session model, all agents — required reading |
| [`../../behavior-change-playbook.md`](../../behavior-change-playbook.md) | How to propagate `AnswerKind` and `ChannelKind` additions |
| [`../../backend/architecture/solution-cqrs-write-rules.md`](../../backend/architecture/solution-cqrs-write-rules.md) | CQRS rules for `GenerateQnAFromSourceCommand` handler |
| [`../../backend/architecture/repository-rules.md`](../../backend/architecture/repository-rules.md) | Feature-scoped module pattern for `Querify.QnA.Portal.Business.SourceIngestion` |
