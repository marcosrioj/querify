# BaseFAQ Agents Area

This folder is reserved for engineering-side agent automation and related documentation.

## Current state

The folder is documentation-only today. There is no checked-in multi-agent runtime under `agents/` yet.

What exists now:

- this readme

What does not exist yet:

- runnable agent orchestration code
- specialist agent implementations
- repository-local agent tooling

## Why this folder exists

BaseFAQ already contains product-side AI under `dotnet/BaseFaq.AI.*`. Keeping `agents/` separate reserves a future space for engineering automation without mixing it into the product runtime.

In other words:

- `dotnet/BaseFaq.AI.*` is part of the product
- `agents/` is a reserved engineering automation area

## If this area grows in the future

Any future implementation here should align with the real repository boundaries:

- `apps/` for frontend work
- `dotnet/` for backend and API work
- `docker/`, `local/`, `azure/`, and `.github/` for platform work
- `docs/` for architecture, release, and operational documentation

## Related documentation

- [`../docs/architecture/basefaq-multi-agent-system.md`](../docs/architecture/basefaq-multi-agent-system.md)
- [`../docs/architecture/solution-architecture.md`](../docs/architecture/solution-architecture.md)
