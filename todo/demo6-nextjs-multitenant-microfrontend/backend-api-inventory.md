# Backend API Inventory For Demo6 Micro-frontend

This inventory only lists the API surface that is relevant to the first Demo6 micro-frontend delivery.

## Verified Sources

- `README.md`
- `dotnet/BaseFaq.Tenant.Portal.Api/Program.cs`
- `dotnet/BaseFaq.Faq.Portal.Api/Program.cs`
- `dotnet/BaseFaq.Faq.Public.Api/Program.cs`
- controller and DTO files under `dotnet/`

## Service Matrix

| Service | Local Base URL | Auth | Extra Header | Demo Usage |
|---|---|---|---|---|
| Tenant Portal API | `http://localhost:5002` | Bearer token | none | tenant list, profile, client key, AI provider setup |
| FAQ Portal API | `http://localhost:5010` | Bearer token | `X-Tenant-Id` | tenant FAQ authoring |
| FAQ Public API | `http://localhost:5020` | none | `X-Client-Key` | public preview, search, vote |
| Tenant BackOffice API | `http://localhost:5000` | Bearer token | none | optional internal/admin-only fallback |

## Current APIs The Demo Can Use In Live Mode

### Tenant Portal API

- `GET /api/tenant/tenants/GetAll`
  - returns `TenantSummaryDto[]`
  - useful for tenant switcher and slug to id lookup
- `POST /api/tenant/tenants/CreateOrUpdate`
  - request: `TenantCreateOrUpdateRequestDto`
  - current shape only supports `name` and `edition`
- `GET /api/tenant/tenants/GetClientKey`
  - returns the current tenant client key for public FAQ access
- `POST /api/tenant/tenants/GenerateNewClientKey`
  - rotates the public client key
- `POST /api/tenant/tenants/SetAiProviderCredentials`
  - request: `TenantSetAiProviderCredentialsRequestDto`
- `GET /api/tenant/aiproviders/GetAll`
  - returns available AI providers
- `GET /api/tenant/tenants/GetConfiguredAiProviders`
  - returns configured provider state for the tenant
- `GET /api/tenant/tenants/IsAiProviderKeyConfigured/{command}`
  - checks whether the key exists for `Generation` or `Matching`
- `GET /api/user/UserProfile`
- `PUT /api/user/UserProfile`

### FAQ Portal API

- `GET /api/faqs/faq`
  - request query is `FaqGetAllRequestDto`
  - supports paging, sorting, and include flags
- `GET /api/faqs/faq/{id}`
- `POST /api/faqs/faq`
- `PUT /api/faqs/faq/{id}`
- `DELETE /api/faqs/faq/{id}`
- `POST /api/faqs/faq/{id}/generation-request`
  - returns `202 Accepted`
- `GET /api/faqs/faq-item`
- `GET /api/faqs/faq-item/{id}`
- `POST /api/faqs/faq-item`
- `PUT /api/faqs/faq-item/{id}`
- `DELETE /api/faqs/faq-item/{id}`

### FAQ Public API

- `GET /api/faqs/faq`
  - request query is `FaqGetAllRequestDto`
  - client key required
- `GET /api/faqs/faq/{id}`
  - query flags come from `FaqGetRequestDto`
- `POST /api/faqs/faq-item`
  - public submission flow currently exists in API surface
- `GET /api/faqs/faq-item/search`
- `POST /api/faqs/vote`

## Useful Current DTOs

### Tenant

- `TenantSummaryDto`
  - `Id`
  - `Slug`
  - `Name`
  - `Edition`
  - `App`
  - `IsActive`
- `TenantEdition`
  - `Free`
  - `Starter`
  - `Pro`
  - `Business`
  - `Enterprise`

### User

- `UserProfileDto`
  - `GivenName`
  - `SurName`
  - `Email`
  - `PhoneNumber`

### FAQ

- `FaqDto`
  - `Id`
  - `Name`
  - `Language`
  - `Status`
  - `SortStrategy`
  - `CtaEnabled`
  - `CtaTarget`
- `FaqDetailDto`
  - same core fields plus optional `Items`, `ContentRefs`, and `Tags`
- `FaqItemDto`
  - `Id`
  - `Question`
  - `ShortAnswer`
  - `Answer`
  - `AdditionalInfo`
  - `CtaTitle`
  - `CtaUrl`
  - `Sort`
  - `VoteScore`
  - `AiConfidenceScore`
  - `IsActive`
  - `FaqId`
  - `ContentRefId`

## Confirmed Header Rules

- `X-Tenant-Id`
  - required by FAQ Portal middleware
  - must be a valid GUID
- `X-Client-Key`
  - required by FAQ Public middleware
  - must be present and non-empty

## Gaps That Require Mock APIs

- Pricing and plan catalog
- Checkout session creation and completion state
- Subscription state and plan change flow
- Payment methods
- Invoice history
- Tenant provisioning state after purchase
- Demo-mode identity/session bootstrap

## Recommendation

For the first runnable frontend delivery:

- use live adapters for Tenant Portal, FAQ Portal, and FAQ Public where possible
- use mock adapters for commerce, provisioning, and identity
- keep the adapter boundary stable so the mock services can later be replaced without rewriting the UI
