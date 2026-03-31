import { run } from '@openai/agents';
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
  const input = `
Project: BaseFaq

Repository context:
${BASEFAQ_DOMAIN_CONTEXT}

Operator request:
${task}

Execution requirements:
- Work in English only.
- Use PR-first delivery.
- Prefer the smallest safe set of changes.
- Name the specialist owner for each deliverable.
- Name the required human approval surface before concluding.
`.trim();

  const result = await run(lead, input);
  const persisted = await persistRunSummary(task, result);

  return {
    finalOutput: result.finalOutput,
    interruptions: result.interruptions ?? [],
    summaryPath: persisted.summaryPath,
    statePath: persisted.statePath,
  };
}
