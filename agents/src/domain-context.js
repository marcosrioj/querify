export const BASEFAQ_REPO_FACTS = {
  productName: 'BaseFaq',
  primaryBackendStack: '.NET 10',
  frontendBaseline: {
    root: 'frontend/demos/metronic-tailwind-react-demos/typescript/nextjs',
    layoutReference:
      'frontend/demos/metronic-tailwind-react-demos/typescript/nextjs/app/components/layouts/demo6',
  },
  apiHosts: [
    'dotnet/BaseFaq.Faq.Portal.Api',
    'dotnet/BaseFaq.Faq.Public.Api',
    'dotnet/BaseFaq.Tenant.BackOffice.Api',
    'dotnet/BaseFaq.Tenant.Portal.Api',
    'dotnet/BaseFaq.AI.Api',
  ],
  aiRuntime: {
    apiRoot: 'dotnet/BaseFaq.AI.Api',
    businessModules: [
      'dotnet/BaseFaq.AI.Business.Common',
      'dotnet/BaseFaq.AI.Business.Generation',
      'dotnet/BaseFaq.AI.Business.Matching',
    ],
    transport: 'RabbitMQ + MassTransit',
  },
  multitenancy: {
    tenantRegistry: 'dotnet/BaseFaq.Common.EntityFramework.Tenant',
    faqPersistence: 'dotnet/BaseFaq.Faq.Common.Persistence.FaqDb',
    tooling: [
      'dotnet/BaseFaq.Tools.Migration',
      'dotnet/BaseFaq.Tools.Seed',
    ],
  },
  platformRoots: ['azure', 'docker', '.github', 'local/env'],
  docsRoot: 'docs',
  uiuxRoot: 'uiux',
};

export const TEAM_OPERATING_RULES = [
  'Work in English only.',
  'Operate with a PR-first workflow.',
  'Never write secrets into the repository or agent traces.',
  'Do not perform direct production changes.',
  'Escalate or require approval for high-risk shell actions.',
  'Respect tenant boundaries and never infer tenant scope from ambient worker state.',
  'Prefer the smallest safe change set that satisfies the task.',
];

export const BASEFAQ_DOMAIN_CONTEXT = `
BaseFaq is a multi-tenant FAQ platform.

Current repository facts:
- Primary backend stack: ${BASEFAQ_REPO_FACTS.primaryBackendStack}
- Frontend baseline: ${BASEFAQ_REPO_FACTS.frontendBaseline.root}
- Demo6 layout reference: ${BASEFAQ_REPO_FACTS.frontendBaseline.layoutReference}
- API hosts: ${BASEFAQ_REPO_FACTS.apiHosts.join(', ')}
- Current AI runtime: ${BASEFAQ_REPO_FACTS.aiRuntime.apiRoot}
- AI business modules: ${BASEFAQ_REPO_FACTS.aiRuntime.businessModules.join(', ')}
- AI transport: ${BASEFAQ_REPO_FACTS.aiRuntime.transport}
- Tenant registry: ${BASEFAQ_REPO_FACTS.multitenancy.tenantRegistry}
- FAQ persistence: ${BASEFAQ_REPO_FACTS.multitenancy.faqPersistence}
- Tenant/data tooling: ${BASEFAQ_REPO_FACTS.multitenancy.tooling.join(', ')}
- Platform roots: ${BASEFAQ_REPO_FACTS.platformRoots.join(', ')}
- Documentation root: ${BASEFAQ_REPO_FACTS.docsRoot}
- UI/UX root: ${BASEFAQ_REPO_FACTS.uiuxRoot}

Repository operating rules:
${TEAM_OPERATING_RULES.map((rule) => `- ${rule}`).join('\n')}
`.trim();

export function formatBaseFaqContext() {
  return BASEFAQ_DOMAIN_CONTEXT;
}
