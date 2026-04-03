import { z } from 'zod';
import { CtaTarget, FaqSortStrategy, FaqStatus } from '@/shared/constants/backend-enums';
import { numericEnumSchema } from '@/shared/lib/zod';

export const faqFormSchema = z.object({
  name: z.string().min(2, 'FAQ name is required.'),
  language: z
    .string()
    .min(2, 'Language is required.')
    .max(16, 'Keep the language code within the backend limit.'),
  status: numericEnumSchema(FaqStatus),
  sortStrategy: numericEnumSchema(FaqSortStrategy),
  ctaEnabled: z.boolean(),
  ctaTarget: numericEnumSchema(CtaTarget),
});

export type FaqFormValues = z.infer<typeof faqFormSchema>;
