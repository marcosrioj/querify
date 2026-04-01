# BaseFaq Product Map For Agents

## Product Summary

BaseFaq is a multi-tenant FAQ platform with separate API surfaces for tenant administration, authenticated portal workflows, public FAQ consumption, and AI-assisted generation/matching.

## Current Solution Inventory

### API hosts

- `dotnet/BaseFaq.Faq.Portal.Api`
- `dotnet/BaseFaq.Faq.Public.Api`
- `dotnet/BaseFaq.Tenant.BackOffice.Api`
- `dotnet/BaseFaq.Tenant.Portal.Api`
- `dotnet/BaseFaq.AI.Api`

### AI worker shape

- `dotnet/BaseFaq.AI.Api` is the current AI composition root.
- `dotnet/BaseFaq.AI.Business.Generation` and `dotnet/BaseFaq.AI.Business.Matching` implement asynchronous generation and matching.
- RabbitMQ and MassTransit already exist in the current AI delivery model.
- The current provider runtime includes OpenAI-compatible, Azure OpenAI, Anthropic, Google, Cohere, and Voyage strategies.

### Multitenancy and persistence

- `dotnet/BaseFaq.Common.EntityFramework.Tenant`: tenant registry and connection management.
- `dotnet/BaseFaq.Faq.Common.Persistence.FaqDb`: FAQ application data ownership.
- `dotnet/BaseFaq.Tools.Migration`: migration runner.
- `dotnet/BaseFaq.Tools.Seed`: seed runner for essential and dummy data.

### Frontend baseline

- Use `frontend/demos/metronic-tailwind-react-demos/typescript/nextjs` as the baseline reference.
- The preferred structural reference for future BaseFaq micro-frontends is the Demo6 layout at:
  `frontend/demos/metronic-tailwind-react-demos/typescript/nextjs/app/components/layouts/demo6`

### Platform and delivery surfaces

- `azure/`: Azure setup and environment orchestration.
- `docker/`: local platform dependencies and observability stack.
- `.github/workflows/`: currently contains deployment automation and should remain the CI/CD root.
- `local/env/`: local certificates and environment simulation assets.

## Delivery Roots By Specialist

- UI/UX agent: `uiux/`
- Frontend agent: `frontend/`
- Backend agent: `dotnet/`
- Multitenancy/Data agent: `dotnet/` and schema/tooling areas
- Platform agent: `azure/`, `.github/`, `docker/`, `local/env/`
- Security/QA agent: `docs/testing/`, test projects, CI quality gates
- Docs/Release agent: `docs/`

## Non-Negotiable Rules

- Work in English only.
- Prefer direct implementation inside owned scopes.
- Do not write secrets into the repository.
- Do not bypass tenant boundaries or infer tenant context from ambient state inside worker processes.
- Do not modify production systems directly from the agent runtime.
- Treat `frontend/demos/` as a reference baseline unless a task explicitly says to evolve the demo source.
