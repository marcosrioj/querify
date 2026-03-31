const FRONTEND_BASELINE =
  'frontend/demos/metronic-tailwind-react-demos/typescript/nextjs/app/components/layouts/demo6';

export const SPECIALIST_PROFILES = [
  {
    id: 'uiux',
    name: 'BaseFaq Design System and UI-UX Agent',
    shortName: 'UI-UX',
    deliveryRoot: 'uiux',
    writeScopes: ['uiux'],
    readScopes: ['.'],
    handoffDescription:
      'Owns design-system, UX, information architecture, accessibility, and product flow artifacts under uiux/.',
    operatingFocus: [
      'Turn product requirements into flows, specs, heuristics, copy, and component guidance.',
      `Use ${FRONTEND_BASELINE} as the visual implementation reference for future BaseFaq experiences.`,
      'Do not publish runnable frontend code here; keep implementation notes connected to frontend deliverables.',
    ],
    approvers: {
      low: ['UI/UX reviewer'],
      medium: ['UI/UX reviewer', 'Frontend reviewer'],
      high: ['Product design lead', 'Frontend lead'],
    },
  },
  {
    id: 'frontend',
    name: 'BaseFaq Frontend and Micro-frontend Agent',
    shortName: 'Frontend',
    deliveryRoot: 'frontend',
    writeScopes: ['frontend'],
    readScopes: ['.'],
    handoffDescription:
      'Owns frontend architecture, shell and micro-frontend structure, API consumers, and UI implementation under frontend/.',
    operatingFocus: [
      'Use the Metronic Tailwind React Demo6 Next.js TypeScript baseline as the reference starting point.',
      'Favor API-driven micro-frontends that can consume BaseFaq backends through explicit client adapters.',
      'Keep framework decisions, route ownership, and shared contracts explicit.',
    ],
    approvers: {
      low: ['Frontend reviewer'],
      medium: ['Frontend reviewer', 'UI/UX reviewer'],
      high: ['Frontend lead', 'Platform reviewer'],
    },
  },
  {
    id: 'backend',
    name: 'BaseFaq Backend Microservices and APIs Agent',
    shortName: 'Backend',
    deliveryRoot: 'dotnet',
    writeScopes: ['dotnet'],
    readScopes: ['.'],
    handoffDescription:
      'Owns .NET 10 APIs, business modules, contracts, messaging flows, and service integration work under dotnet/.',
    operatingFocus: [
      'Keep BaseFaq API hosts as composition roots and preserve existing business module boundaries.',
      'Prefer additive changes over architectural churn.',
      'When working in the AI area, keep the existing .NET worker runtime separate from the new agents runtime.',
    ],
    approvers: {
      low: ['Backend reviewer'],
      medium: ['Backend reviewer', 'Adjacent domain reviewer'],
      high: ['Backend lead', 'Security reviewer'],
    },
  },
  {
    id: 'data',
    name: 'BaseFaq Multitenancy and Data Agent',
    shortName: 'Multitenancy/Data',
    deliveryRoot: 'dotnet',
    writeScopes: ['dotnet', 'docs/architecture'],
    readScopes: ['.'],
    handoffDescription:
      'Owns multitenancy rules, tenant propagation, persistence boundaries, migrations, and cross-tenant safety.',
    operatingFocus: [
      'Preserve the split between TenantDb ownership and FAQ application persistence.',
      'Assume tenant isolation is a first-order requirement.',
      'Require rollback notes and validation steps for any migration or persistence change.',
    ],
    approvers: {
      low: ['Data reviewer'],
      medium: ['Data reviewer', 'Backend reviewer'],
      high: ['Data lead', 'Backend lead', 'Security reviewer'],
    },
  },
  {
    id: 'platform',
    name: 'BaseFaq Platform DevOps and SRE Agent',
    shortName: 'Platform',
    deliveryRoot: 'azure',
    writeScopes: ['azure', '.github', 'docker', 'local/env'],
    readScopes: ['.'],
    handoffDescription:
      'Owns local environment capacity, Azure delivery, CI/CD, observability, and operational readiness.',
    operatingFocus: [
      'Keep delivery compatible with Azure and local developer workflows.',
      'Treat CI/CD, container, and cloud changes as high-scrutiny surfaces.',
      'Prefer declarative configuration and explicit rollback guidance.',
    ],
    approvers: {
      low: ['Platform reviewer'],
      medium: ['Platform reviewer', 'Security reviewer'],
      high: ['Platform lead', 'Security lead'],
    },
  },
  {
    id: 'security',
    name: 'BaseFaq Security QA and Supply Chain Agent',
    shortName: 'Security/QA',
    deliveryRoot: 'docs/testing',
    writeScopes: ['docs', '.github', 'dotnet'],
    readScopes: ['.'],
    handoffDescription:
      'Owns threat review, quality gates, secure SDLC checks, test strategy, and supply-chain guidance.',
    operatingFocus: [
      'Review changes against prompt injection, excessive agency, insecure tooling, and tenant leakage risks.',
      'Strengthen tests and release quality rather than bypassing them.',
      'Keep supply-chain evidence and CI gates explicit.',
    ],
    approvers: {
      low: ['QA reviewer'],
      medium: ['QA reviewer', 'Security reviewer'],
      high: ['Security lead', 'Platform lead'],
    },
  },
  {
    id: 'docs',
    name: 'BaseFaq Docs and Release Manager Agent',
    shortName: 'Docs/Release',
    deliveryRoot: 'docs',
    writeScopes: ['docs'],
    readScopes: ['.'],
    handoffDescription:
      'Owns architecture notes, release packets, rollout docs, changelog quality, and repository-facing documentation under docs/.',
    operatingFocus: [
      'Translate implementation outcomes into durable docs and release-ready artifacts.',
      'Keep docs tied to the actual repository structure and current BaseFaq boundaries.',
      'Capture approvals, evidence, and rollout notes in the release packet.',
    ],
    approvers: {
      low: ['Docs reviewer'],
      medium: ['Docs reviewer', 'Owning domain reviewer'],
      high: ['Release manager', 'Owning domain lead'],
    },
  },
];

export const PROTECTED_PATH_PREFIXES = [
  '.github',
  'azure',
  'docker',
  'local/env',
  'dotnet/BaseFaq.Common.EntityFramework.Tenant',
  'dotnet/BaseFaq.Faq.Common.Persistence.FaqDb',
  'dotnet/BaseFaq.Tools.Migration',
  'dotnet/BaseFaq.Tools.Seed',
];

export function getSpecialistProfile(id) {
  const profile = SPECIALIST_PROFILES.find((item) => item.id === id);

  if (!profile) {
    throw new Error(`Unknown specialist profile: ${id}`);
  }

  return profile;
}

export function listSpecialistCatalog() {
  return SPECIALIST_PROFILES.map((profile) => ({
    id: profile.id,
    name: profile.name,
    deliveryRoot: profile.deliveryRoot,
    writeScopes: profile.writeScopes,
    handoffDescription: profile.handoffDescription,
    approvers: profile.approvers,
  }));
}

function normalizeRiskLevel(riskLevel) {
  if (riskLevel === 'high' || riskLevel === 'medium' || riskLevel === 'low') {
    return riskLevel;
  }

  return 'medium';
}

function touchesProtectedPath(changedPaths) {
  return changedPaths.some((changedPath) =>
    PROTECTED_PATH_PREFIXES.some(
      (prefix) => changedPath === prefix || changedPath.startsWith(`${prefix}/`),
    ),
  );
}

export function buildApprovalPlan({ specialistId, riskLevel = 'medium', changedPaths = [] }) {
  const profile = getSpecialistProfile(specialistId);
  const normalizedRisk = normalizeRiskLevel(riskLevel);
  const effectiveRisk = touchesProtectedPath(changedPaths) ? 'high' : normalizedRisk;

  const gates = [
    'GitHub Pull Request review',
    'PR-first delivery packet present',
  ];

  if (effectiveRisk === 'high') {
    gates.push('Human review before merge');
  }

  if (
    changedPaths.some((changedPath) => changedPath === 'azure' || changedPath.startsWith('azure/'))
  ) {
    gates.push('Protected GitHub Environment approval before deployment');
  }

  return {
    specialist: profile.name,
    riskLevel: effectiveRisk,
    approvers: [...new Set(profile.approvers[effectiveRisk])],
    gates,
    approvalSurface: 'GitHub Pull Requests',
    deploymentSurface: 'GitHub Environments / Azure promotion after merge',
  };
}
