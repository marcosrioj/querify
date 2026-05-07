# Portal Getting Started Guidance

This document is the implementation reference for the Portal's first-run, setup progress, and recommended-next-action guidance. Use it when changing any page that tells the operator what to do next, explains why a record is incomplete, or turns empty workspace data into the next useful action.

The Portal does not implement Getting Started as a separate wizard. It uses real workspace data to choose the next best action inside the normal operating screens.

## Ownership

- UI composition rules still live in [`portal-app-ui-prompt-guidance.md`](portal-app-ui-prompt-guidance.md).
- App architecture and routing rules still live in [`portal-app.md`](portal-app.md).
- This document owns the step order, completion criteria, and inventory of implemented guidance surfaces.
- When any linked page changes its setup, help, empty-state, or recommended-action behavior, update this document in the same change.

## Implementation Anchors

| Surface | Implementation |
|---|---|
| Dashboard activation state, setup percentage, next-action ranking, business readout, account administration guidance, and billing attention flag | [`dashboard-selectors.ts`](../../../apps/portal/src/domains/dashboard/dashboard-selectors.ts) |
| Dashboard setup focus card, queue guidance, business readout rendering, account administration panel, billing notice, and inline empty states | [`dashboard-page.tsx`](../../../apps/portal/src/domains/dashboard/dashboard-page.tsx) |
| Shared detail-page recommended-action card | [`recommended-next-action-card.tsx`](../../../apps/portal/src/domains/qna/recommended-next-action-card.tsx) |
| Shared setup checklist card | [`progress-checklist.tsx`](../../../apps/portal/src/shared/ui/progress-checklist.tsx) |
| Shared form setup wrapper | [`form-setup-progress.tsx`](../../../apps/portal/src/shared/ui/form-setup-progress.tsx) |
| Shared setup completion helpers | [`form-setup-progress-utils.ts`](../../../apps/portal/src/shared/ui/form-setup-progress-utils.ts) |
| Shared empty and error placeholders | [`placeholder-state.tsx`](../../../apps/portal/src/shared/ui/placeholder-state.tsx) |
| Page header hint rendering | [`page-layouts.tsx`](../../../apps/portal/src/shared/layout/page-layouts.tsx) and [`portal-toolbar.tsx`](../../../apps/portal/src/domains/shell/portal-toolbar.tsx) |

## Canonical Activation Model

Dashboard setup progress is computed from five booleans in `getActivationState` and `getSetupProgress`.

| Step | Complete when | Primary route |
|---|---|---|
| Profile | `useUserProfile()` returns a profile with `givenName` | `/app/settings/profile` |
| Space | workspace has at least one Space | `/app/spaces/new` |
| Teammate | member count is greater than one | `/app/members` |
| Question | workspace has at least one Question | first accepting Space, then its question flow |
| Active answer | workspace has at least one active Answer | first active Answer detail |

The dashboard setup focus card renders only while setup progress is below `100%`. No setup-progress or completion card should render after every activation step is complete.

Current behavior: the profile step contributes to setup progress, but `getRoleAwareNextAction` does not currently route directly to profile settings. The next-action ranking is context-oriented after workspace records exist, so it should not be treated as a one-to-one mirror of every progress checkbox.

Sources and accepted answers are optional relationship features. They can add provenance, curation, and canonical selection, but they must not block setup progress or become mandatory next actions.

Active lifecycle status means a Space, Question, or Answer is available for operators, public exposure when visible, and future automation. Active records must not be treated as open demand, unresolved work, or required next actions. Draft records are the lifecycle state that can drive review queues.

## Dashboard Next-Action Ranking

`getRoleAwareNextAction` is the canonical first-run sequence. Earlier rules win over later rules.

| Rank | Condition | Label | Destination |
|---|---|---|---|
| 1 | No Spaces exist | `Start here` | `/app/spaces/new` |
| 2 | Spaces exist but none are active | `Open spaces` | `/app/spaces` |
| 3 | Draft questions exist | `Review draft questions` | first draft Question, otherwise the selected Space |
| 4 | No active Questions exist | `Open Space` | first active Space that accepts questions, otherwise the first active Space |
| 5 | Any active Space has no Questions | `Open Space` | first active Space without Questions |
| 6 | Subscription is past due or unpaid | `Review billing` | `/app/billing` |
| 7 | Member count is one or less | `Invite teammate` | `/app/members` |
| 8 | None of the above | `Review activity` | selected Space |

Billing entitlement problems can also render the dashboard `BillingNotice`. That notice is separate from setup progress and can appear even when the setup focus card is hidden.

## Dashboard Guidance Surfaces

| Surface | What it guides | Implementation |
|---|---|---|
| `SetupFocusCard` | Shows current setup percent and links to `getRoleAwareNextAction` | [`dashboard-page.tsx`](../../../apps/portal/src/domains/dashboard/dashboard-page.tsx) |
| `ExecutiveSummaryCard` | Promotes the highest business readout, repeats the next action, and shows draft questions, reusable questions, active Spaces, and how many active Spaces already have Questions. `Targets to resolve` stays in warning when any active Space has no Questions, and the primary action uses the concrete required action such as `Open Space`. | [`dashboard-page.tsx`](../../../apps/portal/src/domains/dashboard/dashboard-page.tsx) |
| `BusinessReadout` | Explains unresolved targets, reusable active questions, reusable active answers, and optional source visibility | [`dashboard-selectors.ts`](../../../apps/portal/src/domains/dashboard/dashboard-selectors.ts) |
| `Workflow queue` | Shows draft questions that need lifecycle review plus active answers ready for reuse | [`dashboard-page.tsx`](../../../apps/portal/src/domains/dashboard/dashboard-page.tsx) |
| `Account administration` | Shows actionable account work for Billing, Profile, and Settings using the dashboard's existing profile and billing queries | [`dashboard-selectors.ts`](../../../apps/portal/src/domains/dashboard/dashboard-selectors.ts) and [`dashboard-page.tsx`](../../../apps/portal/src/domains/dashboard/dashboard-page.tsx) |
| Inline empty states | Explain that draft questions or active answers will appear after workflow activity exists | [`dashboard-page.tsx`](../../../apps/portal/src/domains/dashboard/dashboard-page.tsx) |

## Detail-Page Recommended Actions

Detail pages use `RecommendedNextActionCard` when the page can infer the next operational move from the record state.

| Page | Rule order |
|---|---|
| [`space-detail-page.tsx`](../../../apps/portal/src/domains/spaces/space-detail-page.tsx) | If the Space is not active, show `Review status` and skip operational target checks. If the active Space blocks questions, show `Review intake rules`. Else if draft questions need action, show `Review draft question`. Else show `Create question`. Missing active answers, accepted answers, or curated sources must not drive the recommended action. |
| [`question-detail-page.tsx`](../../../apps/portal/src/domains/questions/question-detail-page.tsx) | If the Question is draft or archived, show `Review status`. Else show `Review activity`. Missing answers, accepted answer, or source links must not drive the recommended action because active Question status already means the Question is available for use. |
| [`answer-detail-page.tsx`](../../../apps/portal/src/domains/answers/answer-detail-page.tsx) | If the Answer is draft, show `Activate answer`. If archived, show `Reactivate answer`. Else show `Review activity`. Missing source links or accepted state must not drive the recommended action. |
| [`source-detail-page.tsx`](../../../apps/portal/src/domains/sources/source-detail-page.tsx) | Source-specific relationship review can still guide the operator through active Space, Question, and Answer links, but those links are optional and do not make other records incomplete. Draft or archived relationships do not drive missing-link guidance. |
| [`activity-detail-page.tsx`](../../../apps/portal/src/domains/activity/activity-detail-page.tsx) | If the event has answer scope, show `Open answer`. Otherwise show `Open question`. |

Each recommended action should be local and concrete. Prefer scrolling or opening the relevant relationship tab when the action can be completed on the same page.

Operational target checks for Space, Question, and Answer relationships must start from active records only. Draft or archived Spaces, Questions, and Answers can still show lifecycle status actions on their own detail pages, but they must not create missing-relationship targets on dashboards, source guidance, or related surfaces.

## Form Setup Progress

Dedicated create/edit forms use `FormSetupProgressCard`, which wraps `ProgressChecklistCard`. The shared card shows the next incomplete step and hides itself when every step is complete.

| Form page | Steps and completion criteria |
|---|---|
| [`space-form-page.tsx`](../../../apps/portal/src/domains/spaces/space-form-page.tsx) | `Name and slug`: both have at least 2 trimmed characters. `Language`: at least 2 trimmed characters. `Status`: value exists. `Visibility`: value exists. |
| [`source-form-page.tsx`](../../../apps/portal/src/domains/sources/source-form-page.tsx) | `Source type`: value exists. `Locator`: at least 3 trimmed characters. `Language`: at least 2 trimmed characters. `Visibility`: value exists. |
| [`question-form-page.tsx`](../../../apps/portal/src/domains/questions/question-form-page.tsx) | `Space`: value exists. `Title`: at least 3 trimmed characters. `Status`: value exists. `Visibility`: value exists. `Origin channel`: value exists. |
| [`answer-form-page.tsx`](../../../apps/portal/src/domains/answers/answer-form-page.tsx) | `Question`: value exists. `Headline`: at least 3 trimmed characters. `Answer kind`: value exists. `Visibility`: value exists. |
| [`tag-form-page.tsx`](../../../apps/portal/src/domains/tags/tag-form-page.tsx) | `Tag name`: at least 2 trimmed characters. |

Question and Answer create pages also enforce parent context before showing the form:

| Page | Parent prerequisite |
|---|---|
| [`question-form-page.tsx`](../../../apps/portal/src/domains/questions/question-form-page.tsx) | Creating a Question requires opening a Space first. Without a preselected Space, the page shows an empty state that links to `/app/spaces`. |
| [`answer-form-page.tsx`](../../../apps/portal/src/domains/answers/answer-form-page.tsx) | Creating an Answer requires opening a Question first. Without a preselected Question, the page shows an empty state that links to `/app/spaces`. |

## Empty-State And Relationship Guidance

Empty states are part of the first-run model when they explain the missing data and provide the next action.

| Page | Empty guidance currently implemented |
|---|---|
| [`space-list-page.tsx`](../../../apps/portal/src/domains/spaces/space-list-page.tsx) | No spaces in view -> create the first QnA Space. |
| [`source-list-page.tsx`](../../../apps/portal/src/domains/sources/source-list-page.tsx) | No sources in view -> create a Source only when optional references are useful. Relationship-scoped empty state -> open the parent record to manage the optional relationship. |
| [`tag-list-page.tsx`](../../../apps/portal/src/domains/tags/tag-list-page.tsx) | No tags in view -> create the first reusable Tag. Relationship-scoped empty state -> open the parent record to manage the relationship. |
| [`members-page.tsx`](../../../apps/portal/src/domains/members/members-page.tsx) | No members yet -> add the first teammate. No active tenant -> select a workspace first. |
| [`tenant-settings-page.tsx`](../../../apps/portal/src/domains/tenants/tenant-settings-page.tsx) | No active tenant workspace -> create or select a workspace before managing public access keys. |
| [`billing-page.tsx`](../../../apps/portal/src/domains/billing/billing-page.tsx) | No workspace selected, invoices, payments, or entitlement snapshot -> explain what background state is missing. |
| [`space-detail-page.tsx`](../../../apps/portal/src/domains/spaces/space-detail-page.tsx) | No tags, curated sources, questions, or contextual activity -> explain optional taxonomy/source context and required question workflow separately. |
| [`question-detail-page.tsx`](../../../apps/portal/src/domains/questions/question-detail-page.tsx) | No tags, sources, answers, or activity -> explain optional answer/source context separately from the Question lifecycle state. |
| [`answer-detail-page.tsx`](../../../apps/portal/src/domains/answers/answer-detail-page.tsx) | No sources or answer activity -> explain optional references and lifecycle history expectations. |
| [`source-detail-page.tsx`](../../../apps/portal/src/domains/sources/source-detail-page.tsx) | No Spaces, Question links, Answer links, or metadata JSON -> explain optional source reuse and metadata context. |

## Other Explanatory Guidance

These pages do not own the main QnA first-run sequence, but they contain guidance, tips, setup caveats, or explanatory details that should stay aligned with this document when they point users toward a next step.

| Area | Guidance currently implemented |
|---|---|
| [`auth-layout.tsx`](../../../apps/portal/src/domains/auth/auth-layout.tsx) | Static workspace-readiness preview on the unauthenticated shell, including a `Next: Public client key` readiness cue. This is presentation copy, not live setup logic. |
| [`login-page.tsx`](../../../apps/portal/src/domains/auth/login-page.tsx) | Auth0 configuration warning, language selector, and collapsible auth runtime details. |
| [`reset-password-placeholder-page.tsx`](../../../apps/portal/src/domains/auth/reset-password-placeholder-page.tsx) | Explains that password recovery is delegated to Auth0 and links back to sign in. |
| [`profile-settings-page.tsx`](../../../apps/portal/src/domains/settings/profile-settings-page.tsx) | Field descriptions for name, phone, language, and time zone, including language/time-zone resolution hints. |
| [`general-settings-page.tsx`](../../../apps/portal/src/domains/settings/general-settings-page.tsx) | Appearance preference description for the current device. |
| [`security-settings-page.tsx`](../../../apps/portal/src/domains/settings/security-settings-page.tsx) | Session sign-in explanation. |
| [`members-page.tsx`](../../../apps/portal/src/domains/members/members-page.tsx) | Member access explanations, remove-member confirmation, and create-member field descriptions. |
| [`tenant-settings-page.tsx`](../../../apps/portal/src/domains/tenants/tenant-settings-page.tsx) | Workspace metadata descriptions and public client key guidance, including the generate-key confirmation. |
| [`billing-page.tsx`](../../../apps/portal/src/domains/billing/billing-page.tsx) | Billing summary, invoice, payment, and entitlement descriptions, plus empty states for missing billing snapshots. |

## Page Hint Inventory

`descriptionMode="hint"` turns the page description into a compact contextual hint in the unified page trail. These pages currently use that pattern:

- [`dashboard-page.tsx`](../../../apps/portal/src/domains/dashboard/dashboard-page.tsx)
- [`activity-detail-page.tsx`](../../../apps/portal/src/domains/activity/activity-detail-page.tsx)
- [`space-list-page.tsx`](../../../apps/portal/src/domains/spaces/space-list-page.tsx)
- [`space-detail-page.tsx`](../../../apps/portal/src/domains/spaces/space-detail-page.tsx)
- [`space-form-page.tsx`](../../../apps/portal/src/domains/spaces/space-form-page.tsx)
- [`question-detail-page.tsx`](../../../apps/portal/src/domains/questions/question-detail-page.tsx)
- [`question-form-page.tsx`](../../../apps/portal/src/domains/questions/question-form-page.tsx)
- [`answer-detail-page.tsx`](../../../apps/portal/src/domains/answers/answer-detail-page.tsx)
- [`answer-form-page.tsx`](../../../apps/portal/src/domains/answers/answer-form-page.tsx)
- [`source-list-page.tsx`](../../../apps/portal/src/domains/sources/source-list-page.tsx)
- [`source-detail-page.tsx`](../../../apps/portal/src/domains/sources/source-detail-page.tsx)
- [`source-form-page.tsx`](../../../apps/portal/src/domains/sources/source-form-page.tsx)
- [`tag-list-page.tsx`](../../../apps/portal/src/domains/tags/tag-list-page.tsx)
- [`tag-form-page.tsx`](../../../apps/portal/src/domains/tags/tag-form-page.tsx)

Settings, Members, Tenant workspace, and Billing pages also use page descriptions and field descriptions, but they do not currently use the compact page-hint mode.

## Change Checklist

When changing Getting Started or next-action behavior:

1. Update the owning implementation file in the tables above.
2. Keep dashboard setup progress derived from real data and hidden at `100%`.
3. Keep recommended actions action-oriented and route to the nearest place where the user can complete the work.
4. Keep Active status aligned to availability/reuse, not open demand or incomplete work.
5. Keep Source links, Answer links, and accepted-answer selection optional unless the backend contract changes.
6. Update this document with changed steps, criteria, labels, or page inventory.
7. Update localization catalogs when user-facing strings change.
8. Validate loading, empty, error, pending, and complete states using [`../testing/validation-guide.md`](../testing/validation-guide.md).
