import { z } from 'zod';
import {
  ChannelKind,
  QuestionKind,
  QuestionStatus,
  VisibilityScope,
} from '@/shared/constants/backend-enums';
import { numericEnumSchema } from '@/shared/lib/zod';

export const questionFormSchema = z.object({
  spaceId: z.string().min(1, 'Space is required.'),
  title: z.string().min(3, 'Question title is required.'),
  key: z
    .string()
    .min(2, 'Question key is required.')
    .max(64, 'Keep the key within the backend limit.'),
  summary: z.string().max(500, 'Keep the summary concise.').optional(),
  contextNote: z.string().max(2000, 'Keep the context note concise.').optional(),
  threadSummary: z.string().max(2000, 'Keep the thread summary concise.').optional(),
  kind: numericEnumSchema(QuestionKind),
  status: numericEnumSchema(QuestionStatus),
  visibility: numericEnumSchema(VisibilityScope),
  originChannel: numericEnumSchema(ChannelKind),
  language: z.string().max(16, 'Keep the language code within the backend limit.').optional(),
  productScope: z.string().max(200, 'Keep the product scope concise.').optional(),
  journeyScope: z.string().max(200, 'Keep the journey scope concise.').optional(),
  audienceScope: z.string().max(200, 'Keep the audience scope concise.').optional(),
  contextKey: z.string().max(100, 'Keep the context key concise.').optional(),
  originUrl: z.string().max(1000, 'Keep the URL within the backend limit.').optional(),
  originReference: z.string().max(500, 'Keep the reference concise.').optional(),
  confidenceScore: z.coerce.number().int().min(0).max(100),
});

export type QuestionFormValues = z.infer<typeof questionFormSchema>;
