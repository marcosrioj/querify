# Legacy Agent Audit

## Scope Reviewed

- previous BaseFAQ-specific skills under the legacy `.agents/` tree
- generic external specialists under the legacy external specialist tree

## Overlaps

### BaseFAQ skill overlaps

- frontend and UX work was split across separate role folders, but the actual workflows were tightly coupled:
  - page layout
  - portal data flow
  - localization
  - stateful UX feedback
- backend work was split between feature implementation and platform backend concerns, but both still targeted the same BaseFAQ service patterns:
  - CQRS feature modules
  - public tenant-aware reads
  - async AI request publication
  - worker processing
- product and knowledge work overlapped around the FAQ-to-Q&A transition:
  - question-thread modeling
  - provenance design
  - roadmap sequencing

### Generic subagent overlaps

The old external specialist tree contained many near-duplicates:

- multiple backend implementers
- multiple frontend implementers and UI specialists
- multiple data and AI specialists with overlapping scope
- orchestration agents that competed with the main planner role
- reviewer and debugging agents with partially redundant purposes

## Redundancies

- role folders were deeper than necessary for discovery
- there was no single orchestrator file
- there was no normalized skill schema
- generic worker catalogs were wider than the BaseFAQ repository needs

## Gaps

- no single system-level instruction set for intent interpretation
- no precedence rule between skills and subagents
- no shared BaseFAQ glossary or context packet
- no routing matrix for common future prompts
- no BaseFAQ-adapted worker catalog

## Normalization Decisions

- keep BaseFAQ-specific skills as the primary intelligence layer
- move to one path convention: `skills/<category>/<skill>/SKILL.md`
- reduce generic subagents into a smaller worker set aligned to BaseFAQ boundaries
- centralize orchestration, discovery, and terminology under `.agents/`
