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
  deliverable: z.string().min(3).nullable(),
  riskLevel: z.enum(['low', 'medium', 'high']).nullable(),
});

const SpecialistReturnPayload = z.object({
  summary: z.string().min(10),
  changedPaths: z.array(z.string()),
  validation: z.array(z.string()),
  blockers: z.array(z.string()),
  riskLevel: z.enum(['low', 'medium', 'high']),
});

const IMPLEMENTATION_TASK_PATTERN =
  /\b(implement|implementation|build|scaffold|create|runnable|micro-frontend|next\.js|route handler|adapter|app)\b/i;

function isDocumentationPath(path) {
  const normalized = String(path || '').trim().toLowerCase();

  if (!normalized) {
    return true;
  }

  return (
    normalized.endsWith('.md') ||
    normalized.endsWith('.mdx') ||
    normalized.endsWith('.txt') ||
    normalized.endsWith('.rst') ||
    normalized.includes('/readme') ||
    normalized.startsWith('docs/')
  );
}

function requiresImplementationWork(runContext) {
  const taskText = String(runContext?.context?.task || '');
  return IMPLEMENTATION_TASK_PATTERN.test(taskText);
}

function hasNonDocumentationChange(changedPaths) {
  return changedPaths.some((path) => !isDocumentationPath(path));
}

function formatDelegationPayload(profile, payload) {
  const lines = [
    `Agent Lead delegation for ${profile.name}:`,
    `- Goal: ${payload.goal}`,
  ];

  if (payload.deliverable) {
    lines.push(`- Deliverable: ${payload.deliverable}`);
  }

  lines.push(`- Risk level: ${payload.riskLevel || 'medium'}`);
  lines.push(`- Delivery root: ${profile.deliveryRoot}`);

  return lines.join('\n');
}

function extractHandoffPayload(handoffInputData, toolName, payloadSchema) {
  const handoffCall = handoffInputData.newItems.find(
    (item) =>
      item?.type === 'handoff_call_item' &&
      item?.rawItem?.name === toolName &&
      typeof item?.rawItem?.arguments === 'string',
  );

  if (!handoffCall) {
    return null;
  }

  try {
    return payloadSchema.parse(JSON.parse(handoffCall.rawItem.arguments));
  } catch {
    return null;
  }
}

function preserveDelegationContext(profile) {
  return (handoffInputData) => {
    const filtered = removeAllTools(handoffInputData);
    const payload = extractHandoffPayload(
      handoffInputData,
      `delegate_to_${profile.id}_agent`,
      SpecialistHandoffPayload,
    );

    if (!payload) {
      return filtered;
    }

    const delegationNote = formatDelegationPayload(profile, payload);

    if (typeof filtered.inputHistory === 'string') {
      return {
        ...filtered,
        inputHistory: `${filtered.inputHistory}\n\n${delegationNote}`,
      };
    }

    return {
      ...filtered,
      inputHistory: [
        ...filtered.inputHistory,
        {
          role: 'system',
          content: delegationNote,
        },
      ],
    };
  };
}

function formatReturnPayload(profile, payload) {
  const lines = [
    `${profile.name} returned work to the BaseFaq Agent Lead:`,
    `- Summary: ${payload.summary}`,
    `- Risk level: ${payload.riskLevel}`,
    '',
    'Changed paths:',
    ...(payload.changedPaths.length ? payload.changedPaths.map((item) => `- ${item}`) : ['- None']),
    '',
    'Validation:',
    ...(payload.validation.length ? payload.validation.map((item) => `- ${item}`) : ['- None']),
    '',
    'Blockers:',
    ...(payload.blockers.length ? payload.blockers.map((item) => `- ${item}`) : ['- None']),
  ];

  return lines.join('\n');
}

function preserveReturnToLeadContext(profile) {
  return (handoffInputData) => {
    const filtered = removeAllTools(handoffInputData);
    const payload = extractHandoffPayload(
      handoffInputData,
      'return_to_agent_lead',
      SpecialistReturnPayload,
    );

    if (!payload) {
      return filtered;
    }

    const returnNote = formatReturnPayload(profile, payload);

    if (typeof filtered.inputHistory === 'string') {
      return {
        ...filtered,
        inputHistory: `${filtered.inputHistory}\n\n${returnNote}`,
      };
    }

    return {
      ...filtered,
      inputHistory: [
        ...filtered.inputHistory,
        {
          role: 'system',
          content: returnNote,
        },
      ],
    };
  };
}

function buildSpecialistInstructions(profile) {
  return `
${RECOMMENDED_PROMPT_PREFIX}

You are ${profile.name}.

Mission:
- Own the work for your domain and deliver it in English.
- Implement code and file changes directly when the request asks for runnable output and the task fits your writable scope.
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
- Prefer implementation-first delivery over planning-only output when you have enough context to make the change safely.
- When the work is risky, blocked, or needs a durable handoff artifact, record a delivery summary with validation notes, rollback, and follow-up items.
- Do not end the overall run yourself when the Agent Lead is still orchestrating. Return control to the BaseFaq Agent Lead with a concise summary, changed paths, validation notes, blockers, and risk level.
- Use the return_to_agent_lead handoff after finishing your scoped work or when blocked.
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
- Prefer implementation-first delivery when the request asks for code or file changes.
- Consolidate outcomes into a direct implementation result with changed paths, validation notes, blockers, and any required follow-up review guidance.
- Remain the orchestration owner until the run is complete and deliver the final user-facing answer yourself.
- Do not create branches or external review artifacts as part of normal delivery.

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
- Send release documentation and final delivery packaging to the docs/release specialist when documentation is part of the deliverable.
- Keep the current .NET AI worker runtime separate from the agents runtime under agents/.
- When delegating to a specialist, provide a concise handoff payload with goal, deliverable, and risk level.
- Prefer specialists that can write the required code directly over producing planning-only output.
- Request a recorded delivery summary only when the work is risky, blocked, or needs a durable local artifact.
- Expect specialists to return work to you through the return_to_agent_lead handoff instead of concluding the run themselves.
- End with explicit lines that start with "Changed paths:", "Validation:", and "Blockers:" so delivery status is unambiguous.
- Name required human follow-up only when it still matters for protected merges, deployments, or risky areas.
`.trim();
}

function createSpecialistAgent(profile, leadReturnHandoff) {
  const preferredModel =
    profile.id === 'security'
      ? process.env.BASEFAQ_AGENT_SECURITY_MODEL || process.env.BASEFAQ_AGENT_LEAD_MODEL || 'gpt-5'
      : process.env.BASEFAQ_AGENT_SPECIALIST_MODEL || 'gpt-5-mini';

  return new Agent({
    name: profile.name,
    model: preferredModel,
    instructions: buildSpecialistInstructions(profile),
    handoffDescription: profile.handoffDescription,
    handoffs: [leadReturnHandoff],
    tools: createSpecialistTools(profile),
  });
}

function createSpecialistHandoff(profile, specialistAgent) {
  return handoff(specialistAgent, {
    toolNameOverride: `delegate_to_${profile.id}_agent`,
    toolDescriptionOverride: `${profile.handoffDescription} Write only inside ${profile.writeScopes.join(', ')}.`,
    onHandoff: () => {},
    inputType: SpecialistHandoffPayload,
    inputFilter: preserveDelegationContext(profile),
  });
}

function createReturnToLeadHandoff(profile, leadAgent) {
  return handoff(leadAgent, {
    toolNameOverride: 'return_to_agent_lead',
    toolDescriptionOverride:
      'Return control to the BaseFaq Agent Lead with a concise implementation summary, changed paths, validation notes, blockers, and risk level.',
    onHandoff: (runContext, payload) => {
      if (!payload) {
        return;
      }

      if (
        requiresImplementationWork(runContext) &&
        !hasNonDocumentationChange(payload.changedPaths) &&
        payload.blockers.length === 0
      ) {
        throw new Error(
          `${profile.name} cannot return an implementation task with only documentation changes. Create non-documentation files inside your scope or report a concrete blocker.`,
        );
      }
    },
    inputType: SpecialistReturnPayload,
    inputFilter: preserveReturnToLeadContext(profile),
  });
}

export function createBaseFaqTeam() {
  const lead = new Agent({
    name: 'BaseFaq Agent Lead',
    model: process.env.BASEFAQ_AGENT_LEAD_MODEL || 'gpt-5',
    instructions: buildLeadInstructions(),
    handoffs: [],
    tools: createLeadTools(),
    inputGuardrails: [createLeadInputGuardrail()],
    outputGuardrails: [createLeadOutputGuardrail()],
  });

  const specialists = SPECIALIST_PROFILES.map((profile) => {
    const returnToLead = createReturnToLeadHandoff(profile, lead);

    return {
      profile,
      agent: createSpecialistAgent(profile, returnToLead),
    };
  });

  lead.handoffs = specialists.map(({ profile, agent }) => createSpecialistHandoff(profile, agent));

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
      `  Medium-risk reviewers: ${profile.reviewers.medium.join(', ')}`,
    ].join('\n');
  }).join('\n\n');
}
