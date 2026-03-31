# BaseFaq Multi-Agent System

This folder contains an isolated multi-agent runtime for BaseFaq. It does not replace the existing `.NET 10` AI workers in `dotnet/BaseFaq.AI.*`; it adds a separate OpenAI-based engineering team that can inspect the repository, route work to specialists, prepare delivery artifacts, and operate with a strict PR-first workflow.

## Goals

- Keep the agent system isolated in `agents/`.
- Keep every prompt, policy, and runtime-facing instruction in English.
- Route work through one `Agent Lead` plus specialist domain agents.
- Respect the current BaseFaq structure instead of inventing a new one.
- Keep human approvals at the GitHub PR layer and at explicit high-risk gates.

## Team Topology

- `Agent Lead`: intake, decomposition, routing, dependency management, PR packet consolidation.
- `Design System / UI-UX`: owns design and UX artifacts under `uiux/`.
- `Frontend / Micro-frontends`: owns `frontend/` and uses the Demo6 Next.js TypeScript layout as the baseline.
- `Backend / Microservices / APIs`: owns API and business implementation work under `dotnet/`, primarily `.NET 10`.
- `Multitenancy / Data`: owns tenant propagation, persistence boundaries, migrations, and cross-tenant safety.
- `Platform / DevOps / SRE`: owns local environment, Azure, CI, containers, and operational readiness.
- `Security / QA / Supply Chain`: owns test strategy, threat review, supply chain posture, and release quality gates.
- `Docs / Release Manager`: owns documentation and release artifacts under `docs/`.

## BaseFaq Alignment

The runtime is tuned to the repository that exists today:

- `dotnet/BaseFaq.AI.Api` already hosts the current AI worker entrypoint.
- RabbitMQ and MassTransit are already part of the current asynchronous AI execution path.
- `BaseFaq.Common.EntityFramework.Tenant` and `BaseFaq.Faq.Common.Persistence.FaqDb` already encode the multitenant data split.
- `frontend/demos/metronic-tailwind-react-demos/typescript/nextjs` already provides the frontend reference baseline, including the Demo6 layout path.
- `azure/`, `docker/`, and `.github/` already exist as platform and delivery surfaces.

## OpenAI Implementation Notes

This runtime targets the OpenAI Agents SDK and the OpenAI Responses-first direction:

- OpenAI recommends the Responses API as the future direction for agentic workflows: <https://platform.openai.com/docs/guides/responses-vs-chat-completions>
- Reasoning models such as `gpt-5` are recommended for complex planning and agentic work, with `gpt-5-mini` as the lighter specialist default: <https://platform.openai.com/docs/guides/reasoning>
- The Agents SDK supports agents, handoffs, tools, tracing, guardrails, and human-in-the-loop approval flows:
  - <https://openai.github.io/openai-agents-js/>
  - <https://openai.github.io/openai-agents-js/guides/agents/>
  - <https://openai.github.io/openai-agents-js/guides/handoffs/>
  - <https://openai.github.io/openai-agents-js/guides/tools/>
  - <https://openai.github.io/openai-agents-js/guides/tracing/>
  - <https://openai.github.io/openai-agents-js/guides/guardrails/>
  - <https://openai.github.io/openai-agents-js/guides/human-in-the-loop/>

## How To Run

1. `cd agents`
2. Copy `.env.example` into `.env` or export the variables directly.
3. Install dependencies: `npm install`
4. Inspect the team catalog: `npm run catalog`
5. Run an inline task: `npm run run -- --task "Prepare a tenant-safe FAQ generation API refactor"`
6. Run a task from a Markdown file: `npm run run -- --task-file ../docs/requests/faq-agent-refactor.md`
7. You can also pass a single Markdown path positionally: `npm run run -- ../docs/requests/faq-agent-refactor.md`
8. Or pipe a task document on stdin: `cat ../docs/requests/faq-agent-refactor.md | npm run run`

## Task Document Format

- Use Markdown for task packets.
- Keep the file in English when it is intended to be consumed directly by the runtime.
- Start from `templates/task-request-template.md` when you want a repeatable structure.
- If the request came from an IDE chat attachment, convert the relevant content into a `.md` file first and then pass that file with `--task-file`.

## Using From IDEs

### VS Code, Cursor, Windsurf

- Open the integrated terminal in `agents/`.
- Save the task brief as a local `.md` file.
- Run `npm run run -- --task-file ../path/to/your-task.md`.
- If you used the IDE chat first, ask the chat to summarize the request into Markdown and save that result into a file before invoking the runtime.

### JetBrains IDEs

- Create a Node.js run configuration.
- Working directory: the repository `agents/` folder.
- JavaScript file: `src/cli.js`
- Application parameters: `--task-file ../path/to/your-task.md`
- Optional alternative: use the integrated terminal and run the same `npm run run -- --task-file ...` command.

## IDE Chat Attachments

- The current runtime does not ingest IDE-native chat attachments directly.
- Attachments inside the IDE chat stay inside that chat tool unless they are saved to disk.
- The practical workflow is:
  - attach the file in the IDE chat
  - ask the chat to turn the request into a clean Markdown brief
  - save that brief as a `.md` file
  - run the BaseFaq agent team with `--task-file`
- If you want a deeper integration later, the next step would be adding a watch-folder or IDE extension bridge that exports attachment context into Markdown automatically.

## Approval Model

- Agents may inspect the repository and prepare work autonomously.
- High-risk shell actions are configured to require an approval interruption.
- Final code approval belongs in GitHub Pull Requests, not inside the runtime.
- Production deployment approval belongs in protected GitHub environments and the Azure promotion flow after merge.

## Important Files

- `src/cli.js`: command-line entrypoint
- `src/team.js`: agent graph and routing
- `src/tools.js`: local repository tools and audit logging
- `src/gates.js`: risk model, ownership map, and approval matrix
- `context/basefaq-product-map.md`: BaseFaq domain map for the agents
- `policies/pr-first-governance.md`: governance and approval expectations
- `templates/*.md`: reusable work-order, task request, and PR packet templates
