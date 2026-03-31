# BaseFaq Frontend Workspace

This folder is the delivery root for the BaseFaq Frontend and Micro-frontend agent.

## Baseline

Use the current Metronic Tailwind React Next.js TypeScript demo as the implementation baseline:

- `frontend/demos/metronic-tailwind-react-demos/typescript/nextjs`
- Preferred layout reference: `frontend/demos/metronic-tailwind-react-demos/typescript/nextjs/app/components/layouts/demo6`

## Direction

- Build future BaseFaq frontend work as API-driven micro-frontends.
- Keep integration contracts explicit.
- Keep tenant context handling deliberate and minimal in the UI layer.
- Preserve the demos as a reference baseline unless a task explicitly targets the demo source.

## Suggested Structure

- `frontend/microfrontends/`
- `frontend/shared/`
- `frontend/contracts/`
- `frontend/shell/`
