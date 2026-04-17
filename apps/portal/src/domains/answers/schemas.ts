import { z } from 'zod';
import {
  AnswerKind,
  AnswerStatus,
  VisibilityScope,
} from '@/shared/constants/backend-enums';
import { numericEnumSchema } from '@/shared/lib/zod';

export const answerFormSchema = z.object({
  questionId: z.string().min(1, 'Question is required.'),
  headline: z.string().min(3, 'Headline is required.'),
  body: z.string().optional(),
  kind: numericEnumSchema(AnswerKind),
  status: numericEnumSchema(AnswerStatus),
  visibility: numericEnumSchema(VisibilityScope),
  language: z.string().max(16, 'Keep the language code within the backend limit.').optional(),
  contextKey: z.string().max(100, 'Keep the context key concise.').optional(),
  applicabilityRulesJson: z
    .string()
    .max(4000, 'Keep the applicability rules concise.')
    .optional(),
  trustNote: z.string().max(1000, 'Keep the trust note concise.').optional(),
  evidenceSummary: z
    .string()
    .max(2000, 'Keep the evidence summary concise.')
    .optional(),
  authorLabel: z.string().max(120, 'Keep the author label concise.').optional(),
  confidenceScore: z.coerce.number().int().min(0).max(100),
  rank: z.coerce.number().int().min(0),
});

export type AnswerFormValues = z.infer<typeof answerFormSchema>;
