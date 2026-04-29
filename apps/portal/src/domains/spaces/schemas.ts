import { z } from 'zod';
import { SpaceStatus, VisibilityScope } from '@/shared/constants/backend-enums';
import { numericEnumSchema } from '@/shared/lib/zod';

export const spaceFormSchema = z.object({
  name: z.string().min(2, 'Space name is required.'),
  slug: z
    .string()
    .max(64, 'Keep the slug within the backend limit.')
    .optional(),
  language: z
    .string()
    .min(2, 'Language is required.')
    .max(16, 'Keep the language code within the backend limit.'),
  summary: z.string().max(1000, 'Keep the summary concise.').optional(),
  status: numericEnumSchema(SpaceStatus),
  visibility: numericEnumSchema(VisibilityScope),
  acceptsQuestions: z.boolean(),
  acceptsAnswers: z.boolean(),
});

export type SpaceFormValues = z.infer<typeof spaceFormSchema>;
