# Future Work

This folder contains implementation guides for features that are **designed but not yet built**.

## How this folder works

Documents here describe the full target behavior, the implementation path, the gaps that must be
closed first, and a phased roadmap. They exist so that when work starts, the context does not need
to be reconstructed from scratch.

The folder structure mirrors the main `docs/` layout:

```
docs/future/
  integrations/   → planned integrations not yet implemented
  backend/        → planned backend features not yet implemented
  frontend/       → planned frontend features not yet implemented
```

## Lifecycle of a document here

1. **Created here** when a feature is designed but no implementation exists.
2. **Updated here** as decisions are refined or gaps are closed incrementally.
3. **Moved to the appropriate `docs/` subfolder** when the feature is fully implemented and the
   document transitions from a design/roadmap guide to an operational reference.

## What belongs here vs. the main docs

| Belongs in `docs/future/` | Belongs in `docs/` |
|---|---|
| Designed but not yet built | Already implemented and operational |
| Identifies gaps that block implementation | Describes how to use or maintain what exists |
| Contains a roadmap or phased plan | Describes the current production behavior |
| Tracks open decisions | Reflects settled decisions |

Do not put placeholder stubs here. A document belongs here only if it contains enough design
detail to start implementation without additional context.

---

## Current documents

### Backend

- [`backend/source-upload.md`](backend/source-upload.md) — file upload for QnA `Source`: presigned URL flow, MinIO/S3 abstraction, async verification worker, two-phase write (intent → PUT → complete), and a 7-phase implementation roadmap with self-contained agent prompts.

### Integrations

- [`integrations/mcp.md`](integrations/mcp.md) — `BaseFaq.MCP.Server`: multi-agent architecture, one server for all modules, per-agent prompts, tool groups, session model, and roadmap.
- [`integrations/mcp-source-to-qna.md`](integrations/mcp-source-to-qna.md) — Source → Q&A generation pipeline: `qna_import_source`, `GenerateQnAFromSourceCommand`, `ContentFetcher`, `QnAGenerator`.
