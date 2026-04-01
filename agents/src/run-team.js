import { Runner } from '@openai/agents';
import { createHash } from 'node:crypto';
import { promises as fs } from 'node:fs';
import { resolve } from 'node:path';

import { BASEFAQ_DOMAIN_CONTEXT } from './domain-context.js';
import { createBaseFaqTeam } from './team.js';
import { getRuntimePaths } from './tools.js';

function stringifyOutput(value) {
  if (typeof value === 'string') {
    return value;
  }

  return JSON.stringify(value, null, 2);
}

function buildTraceGroupId(task) {
  const hash = createHash('sha256').update(task).digest('hex').slice(0, 24);
  return `basefaq_${hash}`;
}

function getMaxTurns() {
  const configured = Number.parseInt(String(process.env.BASEFAQ_AGENT_MAX_TURNS || ''), 10);

  if (Number.isInteger(configured) && configured >= 1) {
    return configured;
  }

  return 40;
}

function formatProgressTimestamp() {
  return new Date().toISOString().replace('T', ' ').replace('Z', ' UTC');
}

function writeProgressLine(message) {
  process.stderr.write(`[${formatProgressTimestamp()}] ${message}\n`);
}

function normalizeInline(value, maxLength = 120) {
  const text = String(value ?? '')
    .replace(/\s+/g, ' ')
    .trim();

  if (text.length <= maxLength) {
    return text;
  }

  return `${text.slice(0, maxLength - 1)}…`;
}

function parseToolArguments(rawArguments) {
  if (typeof rawArguments !== 'string' || rawArguments.trim() === '') {
    return {};
  }

  try {
    return JSON.parse(rawArguments);
  } catch {
    return { raw: normalizeInline(rawArguments, 80) };
  }
}

function summarizeToolCall(toolName, rawArguments) {
  const args = parseToolArguments(rawArguments);

  switch (toolName) {
    case 'read_repo_file':
    case 'create_or_replace_file':
    case 'replace_text_in_file':
      return args.path ? `path=${args.path}` : '';
    case 'list_repo_files':
      return [args.path ? `path=${args.path}` : '', args.maxDepth != null ? `depth=${args.maxDepth}` : '']
        .filter(Boolean)
        .join(' ');
    case 'search_repo':
      return [
        args.pattern ? `pattern=${JSON.stringify(normalizeInline(args.pattern, 40))}` : '',
        args.path ? `path=${args.path}` : '',
      ]
        .filter(Boolean)
        .join(' ');
    case 'run_repo_command':
      return [
        args.workingDirectory ? `cwd=${args.workingDirectory}` : '',
        args.command ? `command=${JSON.stringify(normalizeInline(args.command, 60))}` : '',
      ]
        .filter(Boolean)
        .join(' ');
    case 'record_delivery_summary':
    case 'record_architecture_decision':
      return args.title ? `title=${JSON.stringify(normalizeInline(args.title, 50))}` : '';
    default: {
      const preview = Object.entries(args)
        .slice(0, 2)
        .map(([key, value]) => `${key}=${JSON.stringify(normalizeInline(typeof value === 'string' ? value : JSON.stringify(value), 30))}`)
        .join(' ');

      return preview;
    }
  }
}

function summarizeToolResult(toolName, result) {
  if (!result || typeof result !== 'object') {
    return '';
  }

  switch (toolName) {
    case 'create_or_replace_file':
    case 'replace_text_in_file':
      return result.path ? `path=${result.path} status=${result.status || 'ok'}` : '';
    case 'record_delivery_summary':
    case 'record_architecture_decision':
      return result.path ? `path=${result.path}` : '';
    case 'run_repo_command':
      return 'completed';
    default:
      return result.path ? `path=${result.path}` : '';
  }
}

function attachProgressReporter(runner) {
  if (process.env.BASEFAQ_AGENT_SHOW_PROGRESS === '0') {
    return;
  }

  let activeAgentName = '';

  runner.on('agent_start', (_context, agent) => {
    if (agent.name === activeAgentName) {
      return;
    }

    activeAgentName = agent.name;
    writeProgressLine(`[agent] ${agent.name}`);
  });

  runner.on('agent_handoff', (_context, fromAgent, toAgent) => {
    activeAgentName = toAgent.name;
    writeProgressLine(`[handoff] ${fromAgent.name} -> ${toAgent.name}`);
  });

  runner.on('agent_tool_start', (_context, agent, tool, details) => {
    const detail = summarizeToolCall(tool.name, details?.toolCall?.arguments);
    writeProgressLine(
      `[tool:start] ${agent.name} -> ${tool.name}${detail ? ` ${detail}` : ''}`,
    );
  });

  runner.on('agent_tool_end', (_context, agent, tool, result) => {
    const detail = summarizeToolResult(tool.name, result);
    writeProgressLine(
      `[tool:end] ${agent.name} <- ${tool.name}${detail ? ` ${detail}` : ''}`,
    );
  });
}

async function persistRunSummary(task, result) {
  const { stateRoot } = getRuntimePaths();
  const runDirectory = resolve(stateRoot, 'runs');
  await fs.mkdir(runDirectory, { recursive: true });

  const timestamp = new Date().toISOString().replace(/[:.]/g, '-');
  const summaryPath = resolve(runDirectory, `${timestamp}.md`);
  const statePath = resolve(runDirectory, `${timestamp}.state.txt`);
  const finalOutput = stringifyOutput(result.finalOutput);

  const lines = [
    '# BaseFaq Multi-Agent Run',
    '',
    `- Timestamp: ${new Date().toISOString()}`,
    '',
    '## Task',
    task,
    '',
    '## Final Output',
    finalOutput,
  ];

  if (result.interruptions?.length) {
    lines.push('', '## Interruptions', ...result.interruptions.map((item) => `- ${JSON.stringify(item)}`));
  }

  await fs.writeFile(summaryPath, lines.join('\n'), 'utf8');

  if (typeof result.state?.toString === 'function') {
    await fs.writeFile(statePath, result.state.toString(), 'utf8');
    return {
      summaryPath,
      statePath,
    };
  }

  return {
    summaryPath,
    statePath: null,
  };
}

export async function runBaseFaqTeam({ task }) {
  const { lead } = createBaseFaqTeam();
  const maxTurns = getMaxTurns();
  const runner = new Runner({
    workflowName: 'BaseFaq Multi-Agent Delivery',
    groupId: buildTraceGroupId(task),
    traceIncludeSensitiveData: process.env.BASEFAQ_AGENT_TRACE_SENSITIVE_DATA === '1',
    traceMetadata: {
      product: 'BaseFaq',
      runtime: 'agents',
      orchestration: 'lead-plus-specialists',
    },
    tracing: process.env.OPENAI_TRACING_EXPORT_API_KEY
      ? { apiKey: process.env.OPENAI_TRACING_EXPORT_API_KEY }
      : undefined,
  });
  attachProgressReporter(runner);
  const input = `
Project: BaseFaq

Repository context:
${BASEFAQ_DOMAIN_CONTEXT}

Operator request:
${task}

Execution requirements:
- Work in English only.
- Prefer direct implementation inside allowed write scopes when the request asks for code or file changes.
- Do not create branches or external review artifacts as part of delivery.
- Prefer the smallest safe set of changes.
- Name the specialist owner for each deliverable.
- End with changed paths, validation, blockers, and any required human follow-up.
`.trim();

  const result = await runner.run(lead, input, {
    maxTurns,
    context: {
      task,
    },
  });
  const persisted = await persistRunSummary(task, result);

  return {
    finalOutput: result.finalOutput,
    interruptions: result.interruptions ?? [],
    summaryPath: persisted.summaryPath,
    statePath: persisted.statePath,
  };
}
