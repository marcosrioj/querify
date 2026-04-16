# Azure Setup (dev / qa / prod)

This folder provides a stage-based Azure setup for BaseFaq.

For the shorter architecture-level summary, start with [`../docs/devops/azure-delivery.md`](../docs/devops/azure-delivery.md).

You choose the stage when running scripts:

- `dev`
- `qa`
- `prod`

Auth0 is manual by design (you fill it in the stage env file).

## Script design

- `lib/common.sh`: shared helpers (validation, env loading, required vars, Azure checks).
- `provision.sh`: infra only.
- `bootstrap-data.sh`: DB bootstrap + seed orchestration.
- `deploy.sh`: image build/push + Container Apps deploy.
- `run-migrations.sh`: applies tenant + FAQ migrations in non-interactive mode.
- `setup.sh`: full flow (`provision -> bootstrap-data -> deploy`).
- `init-env.sh`: creates `env/<stage>.env` from `env/<stage>.env.example`.
- `check-rg.sh`: checks if the stage Resource Group already exists.

## Stage env files

Templates:

- `env/dev.env.example`
- `env/qa.env.example`
- `env/prod.env.example`

Create concrete env files:

```bash
./azure/init-env.sh --stage dev
./azure/init-env.sh --stage qa
./azure/init-env.sh --stage prod
```

Each stage has its own Resource Group (`AZURE_RESOURCE_GROUP` in each stage env).
`provision.sh` will create it if missing, or reuse it if it already exists.

## Domain convention

Configured in templates using `basefaq.com`:

- Dev:
    - `faq.dev.basefaq.com`
    - `faq-public.dev.basefaq.com`
    - `tenant.dev.basefaq.com`
    - `tenant-portal.dev.basefaq.com`
- QA:
    - `faq.qa.basefaq.com`
    - `faq-public.qa.basefaq.com`
    - `tenant.qa.basefaq.com`
    - `tenant-portal.qa.basefaq.com`
- Prod:
    - `faq.basefaq.com`
    - `faq-public.basefaq.com`
    - `tenant.basefaq.com`
    - `tenant-portal.basefaq.com`

## Full setup (simple path)

Run one command per stage:

```bash
./azure/setup.sh --stage dev
./azure/setup.sh --stage qa
./azure/setup.sh --stage prod
```

Check whether a stage RG already exists:

```bash
./azure/check-rg.sh --stage dev
./azure/check-rg.sh --stage qa
./azure/check-rg.sh --stage prod
```

Optional explicit bootstrap mode:

```bash
./azure/setup.sh --stage dev --mode full
./azure/setup.sh --stage qa --mode essential
./azure/setup.sh --stage prod --mode essential
```

Modes:

- `full`: clean + essential + dummy seed (recommended default for dev).
- `essential`: essential seed only (recommended default for qa/prod).
- `dummy`: dummy seed only (requires essential already present).

## Step-by-step flow

```bash
./azure/provision.sh --stage dev
./azure/bootstrap-data.sh --stage dev
./azure/run-migrations.sh --stage dev
./azure/deploy.sh --stage dev
```

`setup.sh` does not call `run-migrations.sh`; use that script explicitly when you want schema migration as a separate stage.

You can override env file path in any script:

```bash
./azure/setup.sh --stage dev --env-file /path/to/dev.env
```

## What is created

- Resource Group
- Container Apps Environment
- Azure Container Registry
- PostgreSQL Flexible Server + DBs
- Azure Cache for Redis
- RabbitMQ on Azure Container Instances
- 4 Container Apps:
    - FAQ Portal API
    - FAQ Public API
    - Tenant BackOffice API
    - Tenant Portal API

## Important notes

- `provision.sh` updates generated values in the stage env file (DB/Redis/RabbitMQ endpoints and secrets).
- `run-migrations.sh` is recommended for CI/CD deployments where you only need DB migration and no seed reset.
- `deploy.sh` uses those values directly as app secrets/env vars.
- Custom domain DNS/CERT binding is still an Azure DNS/SSL operation outside this script.

## GitHub pipeline

Workflow file:

- `.github/workflows/deploy-dev-master.yml`

Trigger:

- every push to `master`
- manual trigger (`workflow_dispatch`)

Behavior:

- prepares `dev` env file at runtime from GitHub `vars` + `secrets`
- runs automatic DB migrations (`run-migrations.sh`)
- deploys only the `dev` APIs (`deploy.sh --stage dev`)

Required GitHub `secrets`:

- `AZURE_CREDENTIALS` (service principal JSON for `azure/login`)
- `TENANT_DB_CONNECTION_STRING`
- `REDIS_PASSWORD`
- `RABBITMQ_PASSWORD`

Required GitHub `vars` (or keep value in `azure/env/dev.env.example`):

- `AZURE_SUBSCRIPTION_ID`
- `AZURE_LOCATION`
- `AZURE_RESOURCE_GROUP`
- `AZURE_CONTAINERAPPS_ENVIRONMENT`
- `AZURE_ACR_NAME`
- `BASEFAQ_ENVIRONMENT`
- `CONTAINERAPP_PREFIX`
- Auth vars (`AUTHORITY_URL`, `AUTH_AUDIENCE`, Swagger auth vars)
- Domain vars (`FAQ_PORTAL_DOMAIN`, `FAQ_PUBLIC_DOMAIN`, `TENANT_BACKOFFICE_DOMAIN`, `TENANT_PORTAL_DOMAIN`,
  no AI-specific domain)
