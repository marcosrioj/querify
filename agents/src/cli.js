import { existsSync, readFileSync, statSync } from 'node:fs';
import { resolve } from 'node:path';
import process from 'node:process';

import { runBaseFaqTeam } from './run-team.js';
import { formatTeamCatalog } from './team.js';
import { getRuntimePaths } from './tools.js';

function loadLocalEnvironment() {
  const envPath = resolve(process.cwd(), '.env');

  if (!existsSync(envPath) || !statSync(envPath).isFile()) {
    return;
  }

  process.loadEnvFile(envPath);
}

function parseArguments(argv) {
  const options = {
    catalog: false,
    task: '',
    taskFile: '',
  };

  for (let index = 0; index < argv.length; index += 1) {
    const token = argv[index];

    if (token === '--catalog') {
      options.catalog = true;
      continue;
    }

    if (token === '--task') {
      options.task = argv[index + 1] || '';
      index += 1;
      continue;
    }

    if (token === '--task-file') {
      options.taskFile = argv[index + 1] || '';
      index += 1;
      continue;
    }
  }

  if (!options.task && !options.taskFile) {
    const positional = argv.filter((token) => !token.startsWith('--'));

    if (positional.length === 1 && looksLikeMarkdownFile(positional[0]) && fileExists(positional[0])) {
      options.taskFile = positional[0];
    } else {
      options.task = positional.join(' ').trim();
    }
  }

  return options;
}

function looksLikeMarkdownFile(candidate) {
  return /\.md$/i.test(candidate);
}

function fileExists(candidate) {
  const absolutePath = resolve(process.cwd(), candidate);

  if (!existsSync(absolutePath)) {
    return false;
  }

  return statSync(absolutePath).isFile();
}

function readTaskFromFile(taskFile) {
  const absolutePath = resolve(process.cwd(), taskFile);

  if (!existsSync(absolutePath) || !statSync(absolutePath).isFile()) {
    throw new Error(`Task file not found: ${absolutePath}`);
  }

  const content = readFileSync(absolutePath, 'utf8').trim();

  if (!content) {
    throw new Error(`Task file is empty: ${absolutePath}`);
  }

  return {
    absolutePath,
    task: `Task source file: ${absolutePath}\n\n${content}`,
  };
}

function readTaskFromStdin() {
  try {
    return readFileSync(0, 'utf8').trim();
  } catch {
    return '';
  }
}

async function main() {
  loadLocalEnvironment();
  const args = parseArguments(process.argv.slice(2));

  if (args.catalog) {
    const paths = getRuntimePaths();
    console.log(`# BaseFaq Multi-Agent Catalog`);
    console.log('');
    console.log(`- Repository root: ${paths.repoRoot}`);
    console.log(`- Agents root: ${paths.agentsRoot}`);
    console.log(`- Local state root: ${paths.stateRoot}`);
    console.log('');
    console.log(formatTeamCatalog());
    return;
  }

  if (args.task && args.taskFile) {
    console.error('Use either `--task` or `--task-file`, not both.');
    process.exitCode = 1;
    return;
  }

  const fileTask = args.taskFile ? readTaskFromFile(args.taskFile) : null;
  const task = fileTask?.task || args.task || readTaskFromStdin();

  if (!task) {
    console.error(
      'No task provided. Use `--task "..."`, `--task-file path/to/request.md`, or pipe a task on stdin.',
    );
    process.exitCode = 1;
    return;
  }

  const result = await runBaseFaqTeam({ task });
  console.log(String(result.finalOutput ?? ''));
  console.log('');

  if (fileTask) {
    console.log(`Task file: ${fileTask.absolutePath}`);
  }

  console.log(`Run summary: ${result.summaryPath}`);

  if (result.statePath) {
    console.log(`Serialized state: ${result.statePath}`);
  }

  if (result.interruptions.length) {
    console.log('');
    console.log('Interruptions detected:');
    result.interruptions.forEach((interruption) => {
      console.log(`- ${JSON.stringify(interruption)}`);
    });
  }
}

main().catch((error) => {
  console.error(error instanceof Error ? error.stack || error.message : String(error));
  process.exitCode = 1;
});
