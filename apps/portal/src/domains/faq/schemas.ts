import { z } from 'zod';
import { FaqStatus } from '@/shared/constants/backend-enums';
import { numericEnumSchema } from '@/shared/lib/zod';

export const faqFormSchema = z.object({
  name: z.string().min(2, 'FAQ name is required.'),
  language: z
    .string()
    .min(2, 'Language is required.')
    .max(16, 'Keep the language code within the backend limit.'),
  status: numericEnumSchema(FaqStatus),
});

export type FaqFormValues = z.infer<typeof faqFormSchema>;
