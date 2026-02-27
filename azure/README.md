# Azure Setup (dev / qa / prod)

This folder provides a stage-based Azure setup for BaseFaq.

You choose the stage when running scripts:

- `dev`
- `qa`
- `prod`

Auth0 is manual by design (you fill it in the stage env file).

## Script design

- `lib/common.sh`: shared helpers (validation, env loading, required vars, Azure checks).
- `provision.sh`: infra only.
- `bootstrap-data.sh`: DB bootstrap + essential seed (`AI_USER_ID` sync).
- `deploy.sh`: image build/push + Container Apps deploy.
- `setup.sh`: full flow (`provision -> bootstrap-data -> deploy`).
- `init-env.sh`: creates `env/<stage>.env` from `env/<stage>.env.example`.

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

## Domain convention

Configured in templates using `basefaq.com`:

- Dev:
    - `faq.dev.basefaq.com`
    - `faq-public.dev.basefaq.com`
    - `tenant.dev.basefaq.com`
    - `tenant-portal.dev.basefaq.com`
    - `ai.dev.basefaq.com`
- QA:
    - `faq.qa.basefaq.com`
    - `faq-public.qa.basefaq.com`
    - `tenant.qa.basefaq.com`
    - `tenant-portal.qa.basefaq.com`
    - `ai.qa.basefaq.com`
- Prod:
    - `faq.basefaq.com`
    - `faq-public.basefaq.com`
    - `tenant.basefaq.com`
    - `tenant-portal.basefaq.com`
    - `ai.basefaq.com`

## Full setup (simple path)

Run one command per stage:

```bash
./azure/setup.sh --stage dev
./azure/setup.sh --stage qa
./azure/setup.sh --stage prod
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
./azure/deploy.sh --stage dev
```

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
- 5 Container Apps:
    - FAQ Portal API
    - FAQ Public API
    - Tenant BackOffice API
    - Tenant Portal API
    - AI API

## Important notes

- `provision.sh` updates generated values in the stage env file (DB/Redis/RabbitMQ endpoints and secrets).
- `bootstrap-data.sh` updates `AI_USER_ID` in the stage env file.
- `deploy.sh` uses those values directly as app secrets/env vars.
- Custom domain DNS/CERT binding is still an Azure DNS/SSL operation outside this script.
