---
name: querify-product-ai
description: "Querify product, module ownership, Creator MVP, and AI product architecture guidance. Use for module boundary decisions, value proposition work, Creator MVP planning, and AI feature placement."
when_to_use: "Use when deciding whether behavior belongs to Tenant, QnA, Direct, Broadcast, or Trust; when discussing Creator MVP; or when adding AI features that might cross product modules."
paths:
  - "docs/business/**"
  - "docs/future/**"
  - "dotnet/Querify.Direct*/**"
  - "dotnet/Querify.Broadcast*/**"
  - "dotnet/Querify.Trust*/**"
  - "dotnet/Querify.Models*/**"
---

# Querify Product And AI

Read:

1. `docs/business/value_proposition/value_proposition.md`
2. `docs/business/value_proposition/ai_product_modules_strategy.md`
3. `docs/business/mvp/creator_mvp_plan.md` for product package and roadmap.
4. `docs/business/mvp/creator_mvp_technical_architecture.md` for implementation ownership.

## Product category

Querify is a Question-to-Knowledge OS. It turns repeated questions, private conversations, public/community interactions, and formal decisions into reusable knowledge and operational learning.

Creator MVP package:

```text
Querify Creator = QnA Answer Hub + Direct Ask Me Inbox + Broadcast Comment Collector + Trust Approval Log
```

## Module ownership

- Tenant: platform control plane: tenants, users, access, billing, entitlements, client keys, module database connections, and operational platform jobs.
- QnA: reusable knowledge: spaces, canonical questions, answers, sources, tags, visibility, workflow, activity, public vote/feedback, and accepted knowledge gaps.
- Direct: private 1:1 interactions: conversations, messages, suggestions, handoff, resolution state, and private gap evidence.
- Broadcast: public or shared interactions: threads, captured items, grouping, public response coordination, community/social signals.
- Trust: validation and governance: review, approval, voting, decisions, rationale, contestation, policy, and audit history.

Use visibility of the interaction to choose Direct vs Broadcast:

- Direct when the response is directed to a person, account, customer, user, or member.
- Broadcast when the communication is visible to many people or belongs in a shared space.

## Handoffs

- Direct -> QnA: private conversation reveals a reusable answer or knowledge gap.
- Broadcast -> QnA: public/community pattern reveals a repeated question or objection.
- Broadcast -> Direct: public interaction requires personal data, negotiation, exception, or private help.
- Direct -> Broadcast: private demand shows that a public answer should be published.
- QnA -> Trust: answer, source, policy, or status needs formal validation or audit.
- Trust -> QnA: decision or validation should become reusable published knowledge.
- Tenant enables or blocks module usage but does not own product workflows.

## AI placement

- AI is a cross-cutting capability, not a sixth product module.
- Product state remains in the owning module.
- AI infrastructure may own prompt versions, model choice, tokens, cost, latency, telemetry, and technical traces.
- QnA is the reliable knowledge foundation before rich Direct or Broadcast automation.
- Search before generation. Use retrieved sources, citations, visibility filters, and tenant isolation.
- Structured output is the default before model output becomes a command.
- Human-in-the-loop comes before automatic public or sensitive actions.
- External content is data, not instruction.
- Evals are required before changing prompts, models, embeddings, chunking, retrieval, or automation policies.

## Do not do now

- Do not create an AI product module.
- Do not put product AI workflows in the Tenant Worker.
- Do not start with a generic chatbot before QnA retrieval and source grounding exist.
- Do not publish AI-generated content directly as active.
- Do not auto-reply publicly without policy, review, metrics, and rollback.
- Do not expose all tools to one unrestricted agent.
