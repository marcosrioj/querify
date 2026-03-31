import { Agent, handoff } from '@openai/agents';
import { RECOMMENDED_PROMPT_PREFIX, removeAllTools } from '@openai/agents-core/extensions';
import { z } from 'zod';

import { BASEFAQ_DOMAIN_CONTEXT, TEAM_OPERATING_RULES } from './domain-context.js';
import { createLeadInputGuardrail, createLeadOutputGuardrail } from './guardrails.js';
import { SPECIALIST_PROFILES, listSpecialistCatalog } from './gates.js';
import { createLeadTools, createSpecialistTools } from './tools.js';

function formatSharedRules() {
  return TEAM_OPERATING_RULES.map((rule) => `- ${rule}`).join('\n');
}

const SpecialistHandoffPayload = z.object({
  goal: z.string().min(5),
  deliverable: z.string().min(3).optional(),
  riskLevel: z.enum(['low', 'medium', 'high']).optional(),
});

function buildSpecialistInstructions(profile) {
  return `
${RECOMMENDED_PROMPT_PREFIX}

You are ${profile.name}.

Mission:
- Own the work for your domain and deliver it in English.
- Publish deliverables only inside your delivery root unless the Agent Lead delegates a cross-domain collaboration.
- Favor precise, low-noise implementation steps over generic advice.

BaseFaq context:
${BASEFAQ_DOMAIN_CONTEXT}

Your delivery root:
- ${profile.deliveryRoot}

Your writable scopes:
${profile.writeScopes.map((item) => `- ${item}`).join('\n')}

Operating focus:
${profile.operatingFocus.map((item) => `- ${item}`).join('\n')}

Mandatory rules:
${formatSharedRules()}
- Do not claim work is complete without naming the changed paths or the missing blockers.
- Do not write outside your writable scopes. If you need another domain, state it and hand back to the Agent Lead.
- When the work produces a merge candidate, create a PR packet with the required approvers and validation notes.
`.trim();
}

function buildLeadInstructions() {
  const catalog = listSpecialistCatalog()
    .map(
      (profile) =>
        `- ${profile.name}: delivery root ${profile.deliveryRoot}; handoff ${profile.handoffDescription}`,
    )
    .join('\n');

  return `
${RECOMMENDED_PROMPT_PREFIX}

You are the BaseFaq Agent Lead.

Mission:
- Read the request.
- Decompose it into the smallest safe domain-owned tasks.
- Route work to specialists using handoffs.
- Consolidate outcomes into a PR-ready recommendation.

BaseFaq context:
${BASEFAQ_DOMAIN_CONTEXT}

Specialist catalog:
${catalog}

Mandatory rules:
${formatSharedRules()}
- Default to one specialist when possible. Use more than one only when the task is truly cross-domain.
- Send multitenancy, migration, and persistence-boundary work to the data specialist.
- Send CI, Azure, container, and observability changes to the platform specialist.
- Send threat review, testing, QA, or supply-chain hardening to the security specialist.
- Send release documentation and final PR packaging to the docs/release specialist when documentation is part of the deliverable.
- Keep the current .NET AI worker runtime separate from the agents runtime under agents/.
- When delegating to a specialist, provide a concise handoff payload with goal, deliverable, and risk level.
- The final answer must name the recommended PR approval surface and the required human gates.
`.trim();
}

function createSpecialistAgent(profile) {
  const preferredModel =
    profile.id === 'security'
      ? process.env.BASEFAQ_AGENT_SECURITY_MODEL || process.env.BASEFAQ_AGENT_LEAD_MODEL || 'gpt-5'
      : process.env.BASEFAQ_AGENT_SPECIALIST_MODEL || 'gpt-5-mini';

  return new Agent({
    name: profile.name,
    model: preferredModel,
    instructions: buildSpecialistInstructions(profile),
    handoffDescription: profile.handoffDescription,
    tools: createSpecialistTools(profile),
  });
}

function createSpecialistHandoff(profile, specialistAgent) {
  return handoff(specialistAgent, {
    toolNameOverride: `delegate_to_${profile.id}_agent`,
    toolDescriptionOverride: `${profile.handoffDescription} Write only inside ${profile.writeScopes.join(', ')}.`,
    inputType: SpecialistHandoffPayload,
    inputFilter: removeAllTools,
  });
}

export function createBaseFaqTeam() {
  const specialists = SPECIALIST_PROFILES.map((profile) => ({
    profile,
    agent: createSpecialistAgent(profile),
  }));

  const handoffs = specialists.map(({ profile, agent }) => createSpecialistHandoff(profile, agent));
  const lead = Agent.create({
    name: 'BaseFaq Agent Lead',
    model: process.env.BASEFAQ_AGENT_LEAD_MODEL || 'gpt-5',
    instructions: buildLeadInstructions(),
    handoffs,
    tools: createLeadTools(),
    inputGuardrails: [createLeadInputGuardrail()],
    outputGuardrails: [createLeadOutputGuardrail()],
  });

  return {
    lead,
    specialists: specialists.map((item) => item.agent),
  };
}

export function formatTeamCatalog() {
  return SPECIALIST_PROFILES.map((profile) => {
    return [
      `${profile.name}`,
      `  Delivery root: ${profile.deliveryRoot}`,
      `  Writable scopes: ${profile.writeScopes.join(', ')}`,
      `  Handoff: ${profile.handoffDescription}`,
      `  Medium-risk approvers: ${profile.approvers.medium.join(', ')}`,
    ].join('\n');
  }).join('\n\n');
}
