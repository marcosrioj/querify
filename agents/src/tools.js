import { tool } from '@openai/agents';
import { execFile } from 'node:child_process';
import { promises as fs } from 'node:fs';
import { dirname, resolve, relative, sep } from 'node:path';
import { fileURLToPath } from 'node:url';
import { promisify } from 'node:util';
import { z } from 'zod';

import { BASEFAQ_DOMAIN_CONTEXT } from './domain-context.js';
import { createToolGuardrails } from './guardrails.js';
import { buildApprovalPlan, listSpecialistCatalog, PROTECTED_PATH_PREFIXES } from './gates.js';

const execFileAsync = promisify(execFile);
const CURRENT_FILE = fileURLToPath(import.meta.url);
const SOURCE_ROOT = dirname(CURRENT_FILE);
const AGENTS_ROOT = resolve(SOURCE_ROOT, '..');
const REPO_ROOT = resolve(AGENTS_ROOT, '..');
const STATE_ROOT = resolve(AGENTS_ROOT, '.state');

const SAFE_COMMAND_PATTERNS = [
  /^pwd$/,
  /^ls(?:\s+.+)?$/,
  /^find\s+.+$/,
  /^rg(?:\s+.+)?$/,
  /^cat\s+.+$/,
  /^sed\s+-n\s+.+$/,
  /^git status(?:\s+--short)?$/,
  /^git diff(?:\s+--stat|\s+--name-only)?$/,
  /^git rev-parse\s+--abbrev-ref HEAD$/,
  /^dotnet build(?:\s+.+)?$/,
  /^dotnet test(?:\s+.+)?$/,
  /^npm run (?:lint|test|build|catalog|check)(?:\s+.+)?$/,
  /^node --check\s+.+$/,
];

const APPROVAL_REQUIRED_COMMAND_PATTERNS = [
  /^git push\b/,
  /^git merge\b/,
  /^gh\b/,
  /^kubectl\b/,
  /^az\b/,
  /^docker\b/,
  /^rm\b/,
  /^mv\b/,
  /^cp\b/,
  /^chmod\b/,
  /^chown\b/,
  /^sudo\b/,
];

function normalizeRelativePath(inputPath = '.') {
  const candidate = String(inputPath).trim();
  const resolvedAbsolute = resolve(REPO_ROOT, candidate);
  const repoRelative = relative(REPO_ROOT, resolvedAbsolute);

  if (repoRelative.startsWith('..') || repoRelative.includes(`..${sep}`)) {
    throw new Error(`Path escapes repository root: ${inputPath}`);
  }

  return repoRelative === '' ? '.' : repoRelative.split(sep).join('/');
}

function resolveRepoPath(relativePath) {
  return resolve(REPO_ROOT, relativePath);
}

function scopeAllows(relativePath, scopes) {
  return scopes.some((scope) => {
    if (scope === '.') {
      return true;
    }

    return relativePath === scope || relativePath.startsWith(`${scope}/`);
  });
}

function assertReadablePath(profile, relativePath) {
  if (!scopeAllows(relativePath, profile.readScopes)) {
    throw new Error(
      `${profile.name} cannot read ${relativePath}. Allowed read scopes: ${profile.readScopes.join(', ')}`,
    );
  }
}

function assertWritablePath(profile, relativePath) {
  if (!scopeAllows(relativePath, profile.writeScopes)) {
    throw new Error(
      `${profile.name} cannot write ${relativePath}. Allowed write scopes: ${profile.writeScopes.join(', ')}`,
    );
  }
}

async function ensureDirectory(targetDirectory) {
  await fs.mkdir(targetDirectory, { recursive: true });
}

function slugify(value) {
  return String(value)
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/^-+|-+$/g, '')
    .slice(0, 80);
}

async function writeTrace(profile, eventName, payload) {
  await ensureDirectory(resolve(STATE_ROOT, 'traces'));
  const dayStamp = new Date().toISOString().slice(0, 10);
  const tracePath = resolve(STATE_ROOT, 'traces', `${dayStamp}.jsonl`);
  const line = JSON.stringify({
    timestamp: new Date().toISOString(),
    specialistId: profile.id,
    specialistName: profile.name,
    eventName,
    payload,
  });

  await fs.appendFile(tracePath, `${line}\n`, 'utf8');
}

function truncateText(value, maxLength = 12000) {
  if (value.length <= maxLength) {
    return value;
  }

  return `${value.slice(0, maxLength)}\n\n<truncated>`;
}

async function listDirectory(relativePath, maxDepth, currentDepth = 0) {
  const absolutePath = resolveRepoPath(relativePath);
  const entries = await fs.readdir(absolutePath, { withFileTypes: true });
  const items = [];

  for (const entry of entries) {
    const entryRelative = relativePath === '.' ? entry.name : `${relativePath}/${entry.name}`;
    items.push({
      path: entryRelative,
      type: entry.isDirectory() ? 'directory' : 'file',
    });

    if (entry.isDirectory() && currentDepth < maxDepth) {
      const nestedItems = await listDirectory(entryRelative, maxDepth, currentDepth + 1);
      items.push(...nestedItems);
    }
  }

  return items;
}

function commandNeedsApproval(command) {
  const normalizedCommand = String(command).trim();

  if (APPROVAL_REQUIRED_COMMAND_PATTERNS.some((pattern) => pattern.test(normalizedCommand))) {
    return true;
  }

  return !SAFE_COMMAND_PATTERNS.some((pattern) => pattern.test(normalizedCommand));
}

function maybeProtectedPath(relativePath) {
  return PROTECTED_PATH_PREFIXES.some(
    (prefix) => relativePath === prefix || relativePath.startsWith(`${prefix}/`),
  );
}

function formatCatalogMarkdown() {
  return listSpecialistCatalog()
    .map((profile) => {
      return [
        `- ${profile.name}`,
        `  Delivery root: ${profile.deliveryRoot}`,
        `  Write scopes: ${profile.writeScopes.join(', ')}`,
        `  Handoff: ${profile.handoffDescription}`,
      ].join('\n');
    })
    .join('\n');
}

export function getRuntimePaths() {
  return {
    repoRoot: REPO_ROOT,
    agentsRoot: AGENTS_ROOT,
    stateRoot: STATE_ROOT,
  };
}

export function createLeadTools() {
  return [
    tool({
      name: 'get_basefaq_context',
      description: 'Return the current BaseFaq repository and architecture context.',
      parameters: z.object({}),
      ...createToolGuardrails({ id: 'lead' }),
      execute: async () => BASEFAQ_DOMAIN_CONTEXT,
    }),
    tool({
      name: 'get_specialist_catalog',
      description: 'Return the BaseFaq specialist catalog, delivery roots, and ownership map.',
      parameters: z.object({}),
      ...createToolGuardrails({ id: 'lead' }),
      execute: async () => formatCatalogMarkdown(),
    }),
    tool({
      name: 'record_architecture_decision',
      description:
        'Persist an architecture or orchestration decision inside agents/.state/decisions for later audit.',
      parameters: z.object({
        title: z.string().min(5),
        rationale: z.string().min(10),
        decision: z.string().min(10),
        riskLevel: z.enum(['low', 'medium', 'high']).default('medium'),
      }),
      ...createToolGuardrails({ id: 'lead' }),
      execute: async ({ title, rationale, decision, riskLevel }) => {
        const decisionDirectory = resolve(STATE_ROOT, 'decisions');
        await ensureDirectory(decisionDirectory);
        const fileName = `${new Date().toISOString().replace(/[:.]/g, '-')}-${slugify(title)}.md`;
        const relativePath = `agents/.state/decisions/${fileName}`;
        const absolutePath = resolve(decisionDirectory, fileName);
        const body = [
          `# ${title}`,
          '',
          `- Timestamp: ${new Date().toISOString()}`,
          `- Risk level: ${riskLevel}`,
          '',
          '## Rationale',
          rationale,
          '',
          '## Decision',
          decision,
        ].join('\n');

        await fs.writeFile(absolutePath, body, 'utf8');
        return { path: relativePath };
      },
    }),
  ];
}

export function createSpecialistTools(profile) {
  const sharedGuardrails = createToolGuardrails(profile);

  return [
    tool({
      name: 'read_repo_file',
      description: 'Read a repository file inside the specialist read scope.',
      parameters: z.object({
        path: z.string().min(1),
      }),
      ...sharedGuardrails,
      execute: async ({ path }) => {
        const relativePath = normalizeRelativePath(path);
        assertReadablePath(profile, relativePath);

        const content = await fs.readFile(resolveRepoPath(relativePath), 'utf8');
        await writeTrace(profile, 'read_repo_file', { path: relativePath });

        return {
          path: relativePath,
          content: truncateText(content),
        };
      },
    }),
    tool({
      name: 'list_repo_files',
      description: 'List files and directories inside the specialist read scope.',
      parameters: z.object({
        path: z.string().default('.'),
        maxDepth: z.number().int().min(0).max(6).default(2),
      }),
      ...sharedGuardrails,
      execute: async ({ path, maxDepth }) => {
        const relativePath = normalizeRelativePath(path);
        assertReadablePath(profile, relativePath);

        const items = await listDirectory(relativePath, maxDepth);
        await writeTrace(profile, 'list_repo_files', { path: relativePath, maxDepth });
        return items;
      },
    }),
    tool({
      name: 'search_repo',
      description: 'Search repository text using ripgrep inside the specialist read scope.',
      parameters: z.object({
        pattern: z.string().min(1),
        path: z.string().default('.'),
        maxMatches: z.number().int().min(1).max(200).default(50),
      }),
      ...sharedGuardrails,
      execute: async ({ pattern, path, maxMatches }) => {
        const relativePath = normalizeRelativePath(path);
        assertReadablePath(profile, relativePath);

        const commandArgs = ['-n', '--no-heading', '--max-count', String(maxMatches), pattern, relativePath];
        let stdout = '';

        try {
          const ripgrepResult = await execFileAsync('rg', commandArgs, {
            cwd: REPO_ROOT,
            maxBuffer: 1024 * 1024,
          });
          stdout = ripgrepResult.stdout;
        } catch (error) {
          if (error?.code === 1) {
            stdout = '';
          } else if (error?.code === 'ENOENT') {
            const grepResult = await execFileAsync(
              'grep',
              ['-R', '-n', '-m', String(maxMatches), pattern, relativePath],
              {
                cwd: REPO_ROOT,
                maxBuffer: 1024 * 1024,
              },
            ).catch((fallbackError) => {
              if (fallbackError?.code === 1) {
                return { stdout: '' };
              }

              throw fallbackError;
            });

            stdout = grepResult.stdout;
          } else {
            throw error;
          }
        }

        await writeTrace(profile, 'search_repo', { pattern, path: relativePath, maxMatches });
        return truncateText(stdout || '<no matches>');
      },
    }),
    tool({
      name: 'run_repo_command',
      description:
        'Run a local repository command. Read-only commands are preferred. High-risk commands require approval.',
      parameters: z.object({
        command: z.string().min(1),
        workingDirectory: z.string().default('.'),
        timeoutSeconds: z.number().int().min(1).max(120).default(30),
      }),
      ...sharedGuardrails,
      needsApproval: async (_context, { command }) => commandNeedsApproval(command),
      execute: async ({ command, workingDirectory, timeoutSeconds }) => {
        const relativePath = normalizeRelativePath(workingDirectory);
        assertReadablePath(profile, relativePath);

        const { stdout, stderr } = await execFileAsync('bash', ['-lc', command], {
          cwd: resolveRepoPath(relativePath),
          timeout: timeoutSeconds * 1000,
          maxBuffer: 1024 * 1024 * 4,
        });

        await writeTrace(profile, 'run_repo_command', {
          command,
          workingDirectory: relativePath,
        });

        return {
          stdout: truncateText(stdout || ''),
          stderr: truncateText(stderr || ''),
        };
      },
    }),
    tool({
      name: 'create_or_replace_file',
      description: 'Create or fully replace a file inside the specialist write scope.',
      parameters: z.object({
        path: z.string().min(1),
        content: z.string(),
      }),
      ...sharedGuardrails,
      execute: async ({ path, content }) => {
        const relativePath = normalizeRelativePath(path);
        assertWritablePath(profile, relativePath);

        const absolutePath = resolveRepoPath(relativePath);
        await ensureDirectory(dirname(absolutePath));
        await fs.writeFile(absolutePath, content, 'utf8');

        await writeTrace(profile, 'create_or_replace_file', {
          path: relativePath,
          protected: maybeProtectedPath(relativePath),
        });

        return { path: relativePath, status: 'written' };
      },
    }),
    tool({
      name: 'replace_text_in_file',
      description: 'Replace a known text block inside a file inside the specialist write scope.',
      parameters: z.object({
        path: z.string().min(1),
        search: z.string().min(1),
        replace: z.string(),
        replaceAll: z.boolean().default(false),
      }),
      ...sharedGuardrails,
      execute: async ({ path, search, replace, replaceAll }) => {
        const relativePath = normalizeRelativePath(path);
        assertWritablePath(profile, relativePath);

        const absolutePath = resolveRepoPath(relativePath);
        const existingContent = await fs.readFile(absolutePath, 'utf8');

        if (!existingContent.includes(search)) {
          throw new Error(`Search text not found in ${relativePath}`);
        }

        const updatedContent = replaceAll
          ? existingContent.split(search).join(replace)
          : existingContent.replace(search, replace);

        await fs.writeFile(absolutePath, updatedContent, 'utf8');
        await writeTrace(profile, 'replace_text_in_file', {
          path: relativePath,
          replaceAll,
          protected: maybeProtectedPath(relativePath),
        });

        return { path: relativePath, status: 'updated' };
      },
    }),
    tool({
      name: 'record_pr_packet',
      description:
        'Create a PR packet under agents/.state/prs with the required approval and validation summary.',
      parameters: z.object({
        title: z.string().min(5),
        summary: z.string().min(10),
        riskLevel: z.enum(['low', 'medium', 'high']).default('medium'),
        changedPaths: z.array(z.string()).default([]),
        validation: z.array(z.string()).default([]),
        rollback: z.string().default('Revert the branch or the merge commit.'),
        followUp: z.array(z.string()).default([]),
      }),
      ...sharedGuardrails,
      execute: async ({
        title,
        summary,
        riskLevel,
        changedPaths,
        validation,
        rollback,
        followUp,
      }) => {
        const normalizedPaths = changedPaths.map((changedPath) => normalizeRelativePath(changedPath));
        const approvalPlan = buildApprovalPlan({
          specialistId: profile.id,
          riskLevel,
          changedPaths: normalizedPaths,
        });

        const prDirectory = resolve(STATE_ROOT, 'prs');
        await ensureDirectory(prDirectory);

        const fileName = `${new Date().toISOString().replace(/[:.]/g, '-')}-${slugify(title)}.md`;
        const absolutePath = resolve(prDirectory, fileName);
        const relativeStatePath = `agents/.state/prs/${fileName}`;
        const markdown = [
          `# ${title}`,
          '',
          `- Specialist: ${profile.name}`,
          `- Risk level: ${approvalPlan.riskLevel}`,
          `- Approval surface: ${approvalPlan.approvalSurface}`,
          `- Deployment surface: ${approvalPlan.deploymentSurface}`,
          '',
          '## Summary',
          summary,
          '',
          '## Changed Paths',
          ...(normalizedPaths.length ? normalizedPaths.map((item) => `- ${item}`) : ['- None supplied']),
          '',
          '## Required Approvers',
          ...approvalPlan.approvers.map((item) => `- ${item}`),
          '',
          '## Blocking Gates',
          ...approvalPlan.gates.map((item) => `- ${item}`),
          '',
          '## Validation',
          ...(validation.length ? validation.map((item) => `- ${item}`) : ['- Validation not provided']),
          '',
          '## Rollback',
          rollback,
          '',
          '## Follow-up',
          ...(followUp.length ? followUp.map((item) => `- ${item}`) : ['- None']),
        ].join('\n');

        await fs.writeFile(absolutePath, markdown, 'utf8');
        await writeTrace(profile, 'record_pr_packet', {
          title,
          path: relativeStatePath,
          riskLevel: approvalPlan.riskLevel,
        });

        return {
          path: relativeStatePath,
          approvers: approvalPlan.approvers,
          gates: approvalPlan.gates,
          approvalSurface: approvalPlan.approvalSurface,
        };
      },
    }),
    tool({
      name: 'get_specialist_policy',
      description: 'Return the specialist ownership, delivery root, and BaseFaq operating context.',
      parameters: z.object({}),
      ...sharedGuardrails,
      execute: async () => {
        return {
          specialist: profile.name,
          deliveryRoot: profile.deliveryRoot,
          writeScopes: profile.writeScopes,
          operatingFocus: profile.operatingFocus,
          approvalModel: profile.approvers,
          context: BASEFAQ_DOMAIN_CONTEXT,
        };
      },
    }),
  ];
}
