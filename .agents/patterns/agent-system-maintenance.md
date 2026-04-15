# Agent System Maintenance

Use this protocol after any non-trivial task.

Its job is to keep `.agents/` synchronized with the real work the repository now requires.

## Rule

If a completed task changes reusable knowledge, `.agents/` must be updated in the same execution flow.

Do not treat `.agents/` maintenance as optional documentation polish.

## Maintenance Triggers

Run the update when at least one of these is true:

1. The prompt exposed a new repeated intent.
   - Example: a class of future prompts now has a stable routing shape.
2. The task changed repository-wide logic or standards.
   - Example: new CQRS rule, new frontend composition rule, new multitenancy rule.
3. The task required reusable coordination between multiple skills.
   - Example: a new common playbook now exists.
4. The task revealed a missing or outdated skill.
   - Example: a recurring capability has no skill or the current skill boundary is wrong.
5. The task revealed a missing or outdated subagent.
   - Example: a bounded execution role now exists or an old worker no longer fits BaseFAQ.
6. The task introduced reusable vocabulary or shared context.
   - Example: a new domain term, platform boundary, or architecture distinction now matters.

## No-Update Cases

You may skip `.agents/` updates only when all of these are true:

- the task was one-off
- no reusable routing or standards changed
- no new skill or worker boundary was revealed
- the result would not help future prompts

## Update Targets

### Update `AGENTS.md` when

- system behavior changes
- precedence rules change
- mandatory execution steps change
- automatic maintenance rules need adjustment

### Update `patterns/intent-routing.md` when

- a new prompt family appears
- a routing rule changes
- conflict resolution needs a new tie-breaker

### Update `patterns/orchestration-playbooks.md` when

- a recurring multi-skill flow appears
- a common playbook gains or loses a supporting skill
- a standard maintenance pass should be appended to a workflow

### Update `shared/*` when

- BaseFAQ context, standards, or repository-level facts changed
- shared generic skills such as `*.skill.md` changed

### Update `glossary/basefaq-glossary.md` when

- a new recurring term or distinction matters for future work

### Update `skills/*` when

- a reusable capability needs a new skill
- a skill description, triggers, responsibilities, or workflow changed
- owned paths or collaborators changed

Also update `skills/README.md` when the skill catalog changes.

### Update `subagents/*` when

- a bounded execution worker is needed
- a worker scope, instructions, or owned paths changed
- a worker became obsolete

Also update `subagents/README.md` when the worker catalog changes.

### Update root `.subagents/**/*` when

- a generic reusable specialist is added or changed
- a low-priority detector or analyzer boundary changes
- a generic specialist becomes obsolete

### Update `templates/*` when

- the standard structure for future skills or workers changed

## Maintenance Sequence

1. Finish the primary task.
2. Ask: "Did this change reusable agent knowledge?"
3. If no, stop.
4. If yes, classify the change:
   - routing
   - orchestration
   - shared standards/context
   - glossary
   - skill
   - subagent
   - template
5. Update the corresponding `.agents/` files.
6. Keep the update minimal but complete.
7. Report that `.agents/` was updated as part of the task.

## Quality Bar

- prefer updating existing files before creating new ones
- create a new skill only when the capability is recurring and distinct
- create a new subagent only when the work is execution-scoped and repeatable
- do not duplicate routing logic across multiple files without a reason
- keep BaseFAQ terminology explicit and consistent
