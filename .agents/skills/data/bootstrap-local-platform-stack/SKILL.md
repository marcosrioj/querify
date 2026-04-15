---
name: bootstrap-local-platform-stack
description: Bring up the BaseFAQ local runtime, infrastructure, app hosts, and observability stack in the supported sequence.
type: repository-skill
scope: basefaq-repository
category: data
priority: medium
triggers:
  - bootstrap local stack
  - run basefaq locally
  - start docker services
  - local observability
owned_paths:
  - README.md
  - docker/*
  - local/*
  - docs/devops/*
  - apps/portal
collaborates_with:
  - apply-seed-and-migrations-safely
---

# Bootstrap Local Platform Stack

## When to Use

- A contributor needs a working BaseFAQ environment for development or debugging.
- A task depends on local Docker services, APIs, the Portal app, or observability tools.

## Responsibilities

- Start infrastructure in the correct order.
- Ensure database initialization happens before dependent APIs run.
- Expose the standard runtime and observability endpoints.

## Workflow

1. Restore and build the solution.
2. Start base services with the repository Docker helper.
3. Run seed or migration tooling before starting dependent APIs.
4. Start the required API and worker hosts.
5. Start the Portal app when frontend flows are needed.
6. Use Jaeger, Prometheus, Grafana, and RabbitMQ UI when diagnosing runtime behavior.

## BaseFAQ Domain Alignment

- Docker is the default path for infra; code iteration usually stays host-run.
- The base-services network must exist before the containerized app stack is started.
- Local debugging should use the built-in observability stack early, not only after failures.

## Collaborates With

- [`apply-seed-and-migrations-safely`](../apply-seed-and-migrations-safely/SKILL.md)

## Done When

- The required infra, services, and frontend are reachable locally.
- Database prerequisites are satisfied before API execution.
- The developer can inspect runtime behavior through the shipped observability tools.
