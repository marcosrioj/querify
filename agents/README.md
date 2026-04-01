# BaseFaq Multi-Agent System

This folder contains an isolated multi-agent runtime for BaseFaq.

In simple terms, this means:

- BaseFaq has a software project.
- This folder adds an AI team that can help plan and execute work on that project.
- That AI team is split into specialized roles instead of acting like one generic chatbot.
- The team works inside the repository, but it does not replace the existing product AI runtime in `dotnet/BaseFaq.AI.*`.

If you are not technical, the easiest way to think about it is:

- `BaseFaq.AI.*` is part of the product.
- `agents/` is part of the engineering team.

The product AI helps BaseFaq users and product workflows.
The multi-agent system helps BaseFaq developers and maintainers.

## Why This Exists

The goal is to let you focus on the business and product decisions while the agent team helps with engineering execution in a structured, safe way.

Instead of one AI trying to do everything, BaseFaq uses:

- one Lead agent to understand and coordinate the work
- several specialist agents to handle design, frontend, backend, data, platform, security, and documentation

This is better for real projects because:

- responsibilities are clearer
- outputs are easier to review
- risky changes can be routed to the correct specialist
- the system can be governed through owned scopes, validation, and human review where it still matters

## What “Multi-Agent” Means Here

In this project, a multi-agent system means:

1. You give the system a task.
2. The `Agent Lead` reads it.
3. The Lead decides which specialist should work on it.
4. That specialist uses safe tools to inspect or edit the repository.
5. The system prepares a result that is ready to be reviewed in a normal engineering workflow.

This is intentionally designed to feel like a real software team:

- the Lead coordinates
- specialists own their domains
- risky work still needs human approval
- final acceptance stays human-controlled

## What It Is Not

This system is not:

- a replacement for human engineering leadership
- a direct production deployment bot
- a free-for-all agent that can change anything anywhere
- a replacement for the existing BaseFaq `.NET 10` AI runtime

## Why The System Lives In `agents/`

The folder is isolated on purpose.

That separation keeps two very different concerns apart:

- `dotnet/BaseFaq.AI.*`
  This is the product-side AI runtime already present in BaseFaq.
  It handles asynchronous generation and matching workflows.

- `agents/`
  This is the engineering-side orchestration runtime.
  It helps the team build, review, document, and evolve the BaseFaq codebase.

This makes the architecture safer and easier to understand.

## How This Fits Into BaseFaq

The runtime is tuned to the repository that exists today.

### Existing BaseFaq backend

BaseFaq already has:

- multiple API hosts under `dotnet/`
- an AI worker API under `dotnet/BaseFaq.AI.Api`
- asynchronous processing with RabbitMQ and MassTransit
- tenant and FAQ persistence boundaries already defined

So the agent system does not invent a new backend architecture.
It respects the current one.

### Existing BaseFaq frontend baseline

BaseFaq already includes a frontend demo baseline:

- `frontend/demos/metronic-tailwind-react-demos/typescript/nextjs`

The preferred reference layout is:

- `frontend/demos/metronic-tailwind-react-demos/typescript/nextjs/app/components/layouts/demo6`

That means the frontend agent does not start from nothing.
It starts from an agreed reference point.

### Existing BaseFaq platform and delivery surfaces

The repository already has:

- `azure/`
- `docker/`
- `.github/`
- `local/env/`

So platform and DevOps work also maps naturally to real folders that already exist.

## Team Topology

This is the agent team.

### 1. Agent Lead

This is the coordinator.

The Lead agent:

- reads the request
- decides what the request really means
- breaks it into smaller pieces
- routes work to the right specialist
- stays in control while specialists return their work
- consolidates the final result
- reports changed paths, validation, blockers, and any required human follow-up

If you are not technical, think of the Lead as the engineering manager or tech lead of the AI team.

### 2. Design System / UI-UX Agent

Owns:

- `uiux/`

This agent is responsible for:

- user flows
- design notes
- accessibility guidance
- interaction decisions
- design-system direction

It does not own runnable frontend code.
It owns the design and UX thinking that should guide implementation.

### 3. Frontend / Micro-frontend Agent

Owns:

- `frontend/`

This agent is responsible for:

- frontend structure
- micro-frontend boundaries
- API-driven UI implementation
- integration with backend APIs

It uses the Demo6 Next.js TypeScript layout as the baseline for future frontend work.

### 4. Backend / Microservices / APIs Agent

Owns:

- `dotnet/`

This agent is responsible for:

- API implementation
- business modules
- service contracts
- messaging flows
- integration logic

It is aligned to `.NET 10`, because that is the main backend technology in BaseFaq today.

### 5. Multitenancy / Data Agent

Owns:

- tenant safety concerns
- persistence boundaries
- migrations
- connection ownership
- cross-tenant risk review

This agent is critical in BaseFaq because BaseFaq is a multi-tenant platform.

That means:

- one tenant must never see another tenant’s data
- data changes must be explicit and safe
- migrations need rollback thinking

### 6. Platform / DevOps / SRE Agent

Owns:

- `azure/`
- `.github/`
- `docker/`
- `local/env/`

This agent is responsible for:

- CI/CD
- Azure delivery
- local environment support
- container and infrastructure conventions
- observability and operational readiness

### 7. Security / QA / Supply Chain Agent

Owns:

- test strategy
- threat review
- secure SDLC checks
- release quality gates
- supply chain posture

This agent helps prevent:

- unsafe changes
- weak testing
- insecure automation
- accidental bypass of engineering controls

### 8. Docs / Release Manager Agent

Owns:

- `docs/`

This agent is responsible for:

- architecture notes
- release packets
- rollout instructions
- changelog quality
- documentation updates

## How A Task Flows Through The System

Here is the real execution flow in plain English.

### Step 1. You provide a task

You can give the system:

- a short inline instruction
- a Markdown brief
- a Markdown file generated from an IDE chat

### Step 2. The Lead agent reads the task

The Lead tries to answer:

- what does the user actually want
- which part of BaseFaq is affected
- how risky is this
- which specialist should own the work

### Step 3. The Lead hands work to a specialist

This uses the OpenAI Agents SDK handoff system.

In practice this means:

- the Lead sends a structured request to one specialist
- the specialist receives a cleaner subset of context
- the specialist works inside its own domain boundaries

### Step 4. The specialist uses tools

The specialist can use repository-local tools to:

- read files
- search files
- list directories
- run allowed local commands
- write only in its owned scopes
- record a delivery summary when a durable handoff artifact is needed

### Step 5. The system produces a direct implementation result

The result may include:

- code or document changes
- a delivery summary
- validation notes
- recommended reviewers for risky work
- rollout or rollback notes

### Step 6. Humans stay on the risky boundaries

This is important.

The agents can prepare work.
They do not replace human merge or deployment authority.

High-risk review still happens in:

- the team's normal human-controlled merge flow

Deployment approval still happens in:

- protected GitHub Environments
- Azure promotion flow

## Why OpenAI Agents SDK Was Chosen

This runtime targets the OpenAI Agents SDK because it is a strong fit for a serious product setup like BaseFaq.

It is useful here because it gives explicit support for:

- agents
- handoffs
- tools
- tracing
- guardrails
- human-in-the-loop approvals

That fits the BaseFaq use case very well:

- one Lead agent
- multiple specialists
- safe repository tools
- traceability
- human approvals for risky actions

Official references:

- Agents SDK overview: <https://openai.github.io/openai-agents-js/>
- Agents guide: <https://openai.github.io/openai-agents-js/guides/agents/>
- Handoffs guide: <https://openai.github.io/openai-agents-js/guides/handoffs/>
- Tools guide: <https://openai.github.io/openai-agents-js/guides/tools/>
- Tracing guide: <https://openai.github.io/openai-agents-js/guides/tracing/>
- Guardrails guide: <https://openai.github.io/openai-agents-js/guides/guardrails/>
- Human-in-the-loop guide: <https://openai.github.io/openai-agents-js/guides/human-in-the-loop/>
- Responses API direction: <https://platform.openai.com/docs/guides/responses-vs-chat-completions>
- Reasoning model guidance: <https://platform.openai.com/docs/guides/reasoning>

## Applied SDK Capabilities

This is what is already applied in the current runtime.

### Agents

The runtime defines:

- one Lead agent
- multiple specialist agents

### Handoffs

The Lead delegates using explicit SDK handoffs with structured payloads.

That matters because it avoids sloppy delegation and makes each specialist receive a cleaner and more focused task.

### Tools

The runtime exposes custom repository-local tools with domain write scopes.

That means a specialist can work, but only inside the areas it is supposed to own.

### Tracing

Each run includes:

- workflow name
- trace group id
- trace metadata
- optional OpenAI tracing export key

By default, sensitive trace data is not included.

### Guardrails

The runtime includes:

- Lead input guardrail
- Lead output guardrail
- tool input guardrails
- tool output guardrails

These guardrails help block:

- obvious secrets
- attempts to bypass production or human safety controls
- risky tool usage
- sensitive output leakage

### Human-in-the-loop approvals

Sensitive tool actions can trigger approval interruptions.

That means the system can stop and ask for approval before doing something riskier.

## Real-World Safety Model

This system is designed for real engineering work, not a demo-only environment.

### What agents are allowed to do

- inspect the repository
- plan work
- route work to specialists
- edit files in approved scopes
- implement code in approved scopes
- record delivery-ready summaries when needed

### What agents are not allowed to do by default

- deploy directly to production
- bypass required human review for risky rollout work
- ignore tenant boundaries
- freely move across all folders without ownership
- expose secrets in traces or outputs

### High-risk work still requires human review

Examples:

- breaking API changes
- multitenant data changes
- migrations
- Azure and CI/CD changes
- secret-management changes
- security-sensitive authentication or authorization changes

## Repository Map For A Non-Technical Reader

Here is the simplest way to understand the main folders involved.

- `agents/`
  The engineering AI team runtime

- `dotnet/`
  The main BaseFaq backend code

- `frontend/`
  The frontend workspace and demo baseline

- `uiux/`
  Design and UX deliverables

- `docs/`
  Architecture, release, and operational documentation

- `azure/`
  Azure delivery and environment setup

- `docker/`
  Local infrastructure and support services

- `.github/`
  CI/CD automation

## How To Run

### Basic setup

1. `cd agents`
2. Copy `.env.example` into `.env`, or export the variables directly.
3. Install dependencies with `npm install`.
4. Inspect the available team with `npm run catalog`.

### Run with a short inline task

```bash
npm run run -- --task "Prepare a tenant-safe FAQ generation API refactor"
```

### Run from a Markdown task document

```bash
npm run run -- --task-file ../docs/requests/faq-agent-refactor.md
```

### Run by passing a single Markdown path

```bash
npm run run -- ../docs/requests/faq-agent-refactor.md
```

### Run by piping a document into stdin

```bash
cat ../docs/requests/faq-agent-refactor.md | npm run run
```

## Environment Variables Explained

This section is intentionally simple.

### `OPENAI_API_KEY`

Your OpenAI API key.

Without this, the runtime cannot call OpenAI.

### `BASEFAQ_AGENT_LEAD_MODEL`

Which model the Lead agent uses.

Default intent:

- use a stronger reasoning model here because the Lead does the coordination work

### `BASEFAQ_AGENT_SPECIALIST_MODEL`

Which model specialists use by default.

Default intent:

- use a lighter model for day-to-day specialist execution

### `BASEFAQ_AGENT_SECURITY_MODEL`

Optional override for the Security/QA agent.

Why this exists:

- security review may deserve a stronger model than routine specialist work

### `BASEFAQ_AGENT_MAX_TURNS`

Maximum orchestration turns allowed for a run before the SDK stops execution.

Use a higher value for larger implementation tasks that require more repository discovery and tool work.

### `OPENAI_TRACING_EXPORT_API_KEY`

Optional tracing export key if you want traces exported through OpenAI tracing.

### `BASEFAQ_AGENT_TRACE_SENSITIVE_DATA`

Controls whether sensitive inputs and outputs should be included in traces.

Default recommendation:

- leave this off

### `BASEFAQ_AGENT_SHOW_PROGRESS`

Controls whether the CLI prints live orchestration progress to stderr while the Lead and specialists work.

Default recommendation:

- leave this on

## Task Document Format

Use Markdown for task packets.

Recommendations:

- write the task in English if the runtime will consume it directly
- keep the objective clear
- include scope and constraints
- include acceptance criteria

Start from:

- `templates/task-request-template.md`

This helps the Lead understand the task faster and route it more accurately.

## Using This From IDEs

The easiest mental model is:

- use the IDE chat to think
- use a Markdown file to formalize the request
- use the `agents/` runtime to execute that request as an AI team

### VS Code, Cursor, Windsurf

Recommended flow:

1. Open the repository.
2. Open the integrated terminal in `agents/`.
3. Create or save a Markdown task brief.
4. Run the task with `--task-file`.

Example:

```bash
npm run run -- --task-file ../docs/requests/my-task.md
```

### JetBrains IDEs

You can use either:

- the integrated terminal
- a Node.js run configuration

Suggested run configuration:

- Working directory: `agents/`
- JavaScript file: `src/cli.js`
- Application parameters: `--task-file ../docs/requests/my-task.md`

## IDE Chat Attachments

The runtime does not directly ingest IDE-native chat attachments.

This is an important practical detail.

What this means:

- if you attach a file to an IDE chat, that file is available to the chat tool
- it is not automatically available to this runtime

Recommended workflow:

1. Attach the file in the IDE chat.
2. Ask the IDE chat to summarize the content into a clean Markdown brief.
3. Save that brief as a local `.md` file.
4. Run the BaseFaq agent team with `--task-file`.

So the IDE chat is the place where you collect and organize attachment context.
The `agents/` runtime is the place where you execute that context as a structured AI team.

## Example For BaseFaq

Imagine you want to improve FAQ generation for tenants.

You might create a Markdown file like this:

```md
# Task Request

## Objective
Improve the FAQ generation flow so it remains tenant-safe and easier to monitor.

## Scope
- AI generation API
- async callbacks
- docs update

## Constraints
- keep current .NET 10 boundaries
- keep RabbitMQ + MassTransit
- do not break tenant isolation

## Acceptance Criteria
- [ ] generation flow documented
- [ ] tenant propagation reviewed
- [ ] required human follow-up clearly identified
```

What would happen next:

- the Lead reads the task
- the Backend agent may inspect `dotnet/BaseFaq.AI.*`
- the Data agent may review tenant safety
- the Docs agent may update documentation
- the final response should say what changed, how it was validated, and whether any human follow-up is still needed

## Review And Rollout Model

This is the most important operational rule.

### Where high-risk review happens

- the team's normal human-controlled merge flow

### Where deployment approval happens

- protected GitHub Environments
- Azure promotion after merge

### Why this matters

The agents are allowed to help.
They are not allowed to become the final authority.

## Important Files

- `src/cli.js`
  Command-line entrypoint

- `src/team.js`
  Defines the Lead and specialist agents

- `src/guardrails.js`
  Defines input, output, and tool guardrails

- `src/tools.js`
  Defines repository-local tools and domain boundaries

- `src/gates.js`
  Defines ownership and review expectations

- `src/run-team.js`
  Runs the orchestration and tracing configuration

- `context/basefaq-product-map.md`
  Explains how the runtime maps to the BaseFaq repository

- `policies/implementation-governance.md`
  Explains direct-implementation governance rules

- `templates/*.md`
  Reusable task, work-order, delivery-summary, and release templates

## Short Glossary

### Agent

An AI role with a clear responsibility.

### Lead agent

The coordinator of the AI team.

### Specialist agent

An AI role focused on one area such as backend or frontend.

### Handoff

The act of the Lead delegating work to a specialist.

### Guardrail

A safety rule that blocks or constrains unsafe behavior.

### Trace

A recorded execution trail that helps explain what happened during a run.

### Direct implementation

A working model where agents implement changes directly in the repository, then report changed paths, validation, blockers, and any required human follow-up.

## Final Mental Model

If you want the shortest possible explanation:

- BaseFaq has a normal software repository.
- `agents/` adds an AI engineering team to help operate inside that repository.
- The Lead agent coordinates specialists.
- Specialists work only in their domains.
- Safety rules limit what can happen automatically.
- Humans still review high-risk changes and control deployments.
