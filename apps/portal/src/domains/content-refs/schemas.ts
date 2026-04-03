import { z } from 'zod';
import { ContentRefKind } from '@/shared/constants/backend-enums';
import { numericEnumSchema } from '@/shared/lib/zod';

export const contentRefFormSchema = z.object({
  kind: numericEnumSchema(ContentRefKind),
  locator: z.string().min(3, 'Locator is required.'),
  label: z.string().optional(),
  scope: z.string().optional(),
});

export type ContentRefFormValues = z.infer<typeof contentRefFormSchema>;
