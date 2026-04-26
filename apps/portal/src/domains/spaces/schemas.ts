import { z } from 'zod';
import { SpaceKind, VisibilityScope } from '@/shared/constants/backend-enums';
import { numericEnumSchema } from '@/shared/lib/zod';

export const spaceFormSchema = z.object({
  name: z.string().min(2, 'Space name is required.'),
  key: z
    .string()
    .min(2, 'Space key is required.')
    .max(64, 'Keep the key within the backend limit.'),
  language: z
    .string()
    .min(2, 'Language is required.')
    .max(16, 'Keep the language code within the backend limit.'),
  summary: z.string().max(1000, 'Keep the summary concise.').optional(),
  kind: numericEnumSchema(SpaceKind),
  visibility: numericEnumSchema(VisibilityScope),
  acceptsQuestions: z.boolean(),
  acceptsAnswers: z.boolean(),
  markValidated: z.boolean(),
});

export type SpaceFormValues = z.infer<typeof spaceFormSchema>;
