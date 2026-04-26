import { z } from 'zod';
import {
  ChannelKind,
  QuestionStatus,
  VisibilityScope,
} from '@/shared/constants/backend-enums';
import { numericEnumSchema } from '@/shared/lib/zod';

export const questionFormSchema = z.object({
  spaceId: z.string().min(1, 'Space is required.'),
  title: z.string().min(3, 'Question title is required.'),
  summary: z.string().max(500, 'Keep the summary concise.').optional(),
  contextNote: z.string().max(2000, 'Keep the context note concise.').optional(),
  status: numericEnumSchema(QuestionStatus),
  visibility: numericEnumSchema(VisibilityScope),
  originChannel: numericEnumSchema(ChannelKind),
  aiConfidenceScore: z.coerce.number().int().min(0).max(100),
  feedbackScore: z.coerce.number().int(),
  sort: z.coerce.number().int().min(0),
});

export type QuestionFormValues = z.infer<typeof questionFormSchema>;
