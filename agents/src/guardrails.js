import {
  ToolGuardrailFunctionOutputFactory,
  defineToolInputGuardrail,
  defineToolOutputGuardrail,
} from '@openai/agents';

const SECRET_PATTERNS = [
  /sk-[a-z0-9]{16,}/i,
  /-----begin [a-z ]*private key-----/i,
  /\bapi[_-]?key\b/i,
  /\bsecret\b/i,
  /\btoken\b/i,
  /\bpassword\b/i,
];

const UNSAFE_DELIVERY_PATTERNS = [
  /deploy\s+directly\s+to\s+prod/i,
  /push\s+straight\s+to\s+main/i,
  /apply\s+directly\s+in\s+production/i,
  /skip\s+production\s+approval/i,
];

const DANGEROUS_COMMAND_PATTERNS = [
  /\bcurl\b/i,
  /\bwget\b/i,
  /\bssh\b/i,
  /\bscp\b/i,
  /\brsync\b/i,
  /\bnc\b/i,
  /\bnetcat\b/i,
];

function normalizeText(value) {
  if (typeof value === 'string') {
    return value;
  }

  if (value == null) {
    return '';
  }

  try {
    return JSON.stringify(value);
  } catch {
    return String(value);
  }
}

export function containsSensitiveData(value) {
  const text = normalizeText(value);
  return SECRET_PATTERNS.some((pattern) => pattern.test(text));
}

export function containsProductionBypassInstruction(value) {
  const text = normalizeText(value);
  return UNSAFE_DELIVERY_PATTERNS.some((pattern) => pattern.test(text));
}

function containsDangerousNetworkCommand(value) {
  const text = normalizeText(value);
  return DANGEROUS_COMMAND_PATTERNS.some((pattern) => pattern.test(text));
}

export function createLeadInputGuardrail() {
  return {
    name: 'BaseFaq Lead Input Guardrail',
    runInParallel: false,
    execute: async ({ input }) => {
      const normalized = normalizeText(input);

      if (containsSensitiveData(normalized)) {
        return {
          outputInfo: 'Sensitive material detected in the request payload.',
          tripwireTriggered: true,
        };
      }

      if (containsProductionBypassInstruction(normalized)) {
        return {
          outputInfo: 'The request attempted to bypass production or human safety controls.',
          tripwireTriggered: true,
        };
      }

      return {
        outputInfo: 'Input accepted.',
        tripwireTriggered: false,
      };
    },
  };
}

export function createLeadOutputGuardrail() {
  return {
    name: 'BaseFaq Lead Output Guardrail',
    execute: async ({ agentOutput }) => {
      const normalized = normalizeText(agentOutput);
      const mentionsChangedPaths = /\bchanged paths?\b/i.test(normalized);
      const mentionsValidation = /\bvalidation\b|\btests?\b|\bblockers\b/i.test(normalized);

      if (!mentionsChangedPaths || !mentionsValidation) {
        return {
          outputInfo:
            'The final answer must include changed paths plus validation or blocker information.',
          tripwireTriggered: true,
        };
      }

      if (containsSensitiveData(normalized)) {
        return {
          outputInfo: 'The final answer appears to expose sensitive data.',
          tripwireTriggered: true,
        };
      }

      return {
        outputInfo: 'Output accepted.',
        tripwireTriggered: false,
      };
    },
  };
}

export function createToolGuardrails(profile) {
  const blockSensitiveInputs = defineToolInputGuardrail({
    name: `${profile.id}_tool_input_guardrail`,
    run: async ({ toolCall }) => {
      const serializedArguments = toolCall?.arguments ?? '';

      if (containsSensitiveData(serializedArguments)) {
        return ToolGuardrailFunctionOutputFactory.rejectContent(
          'Remove secrets or credentials before calling this tool.',
        );
      }

      if (
        toolCall?.name === 'run_repo_command' &&
        containsDangerousNetworkCommand(serializedArguments)
      ) {
        return ToolGuardrailFunctionOutputFactory.rejectContent(
          'Network-capable shell commands are blocked in this runtime. Use repository-local commands only.',
        );
      }

      return ToolGuardrailFunctionOutputFactory.allow();
    },
  });

  const redactSensitiveOutputs = defineToolOutputGuardrail({
    name: `${profile.id}_tool_output_guardrail`,
    run: async ({ output }) => {
      if (containsSensitiveData(output)) {
        return ToolGuardrailFunctionOutputFactory.rejectContent(
          'The tool output appears to contain sensitive data and has been blocked.',
        );
      }

      return ToolGuardrailFunctionOutputFactory.allow();
    },
  });

  return {
    inputGuardrails: [blockSensitiveInputs],
    outputGuardrails: [redactSensitiveOutputs],
  };
}
