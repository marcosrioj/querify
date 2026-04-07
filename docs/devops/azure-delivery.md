# Azure Delivery

## Purpose

This guide summarizes the Azure deployment automation that lives in `azure/`. It is the `docs/` entrypoint for understanding how BaseFAQ is provisioned and deployed across stages.

## Stage model

The Azure scripts are organized around three stages:

- `dev`
- `qa`
- `prod`

Each stage has its own environment file and its own resource group.

## Main scripts

| Script | Responsibility |
|---|---|
| `azure/init-env.sh` | create `env/<stage>.env` from the matching template |
| `azure/provision.sh` | provision infrastructure |
| `azure/bootstrap-data.sh` | bootstrap databases and run essential seed tasks |
| `azure/run-migrations.sh` | apply migrations in non-interactive mode |
| `azure/deploy.sh` | build, push, and deploy the containerized services |
| `azure/setup.sh` | orchestrate the full stage flow |
| `azure/check-rg.sh` | verify whether the resource group already exists |

## Environment files

Templates:

- `azure/env/dev.env.example`
- `azure/env/qa.env.example`
- `azure/env/prod.env.example`

Create a concrete env file:

```bash
./azure/init-env.sh --stage dev
```

Repeat with `qa` or `prod` as needed.

## Standard deployment paths

### Full stage setup

```bash
./azure/setup.sh --stage dev
./azure/setup.sh --stage qa
./azure/setup.sh --stage prod
```

### Explicit phase-by-phase flow

```bash
./azure/provision.sh --stage dev
./azure/bootstrap-data.sh --stage dev
./azure/run-migrations.sh --stage dev
./azure/deploy.sh --stage dev
```

## Setup modes

`setup.sh` supports different data bootstrap modes:

- `full`: clean plus essential plus sample data, best for `dev`
- `essential`: essential data only, typical for `qa` and `prod`
- `dummy`: sample data only, assumes essential data is already present

Example:

```bash
./azure/setup.sh --stage dev --mode full
```

## What gets provisioned

The current Azure flow creates or reuses:

- the stage resource group
- Container Apps environment
- Azure Container Registry
- PostgreSQL Flexible Server and databases
- Azure Cache for Redis
- RabbitMQ on Azure Container Instances
- Container Apps for the public-facing APIs and AI worker host

## Stage domains

The templates follow the `basefaq.com` domain convention:

- `dev`: `*.dev.basefaq.com`
- `qa`: `*.qa.basefaq.com`
- `prod`: `*.basefaq.com`

DNS and certificate binding remain an Azure-side operation outside the shell scripts.

## Operational notes

- `provision.sh` may update generated values inside the stage env file.
- `bootstrap-data.sh` also updates `AI_USER_ID` after essential data is created.
- `run-migrations.sh` is the safest script to reuse in deployment flows when only schema changes are needed.
- Auth0 configuration is intentionally manual and stage-specific.

## CI/CD linkage

The repository contains a GitHub Actions workflow for the `dev` stage:

- `.github/workflows/deploy-dev-master.yml`

Its role is to:

- assemble the `dev` environment at runtime from GitHub variables and secrets
- run migrations
- deploy the `dev` services

## When to use this guide versus `azure/README.md`

Use this document to understand the delivery model quickly. Use `azure/README.md` when you need the raw script inventory and stage-env details directly beside the deployment scripts.
