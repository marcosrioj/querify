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
  contextNote: z.string().max(2000, 'Keep the context note concise.').optional(),
  authorLabel: z.string().max(120, 'Keep the author label concise.').optional(),
  sort: z.coerce.number().int().min(0),
});

export type AnswerFormValues = z.infer<typeof answerFormSchema>;
