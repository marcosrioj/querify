import { z } from 'zod';

export const faqItemFormSchema = z.object({
  question: z.string().min(3, 'Question is required.'),
  additionalInfo: z.string().optional(),
  ctaTitle: z.string().optional(),
  ctaUrl: z
    .string()
    .optional()
    .refine((value) => !value || /^https?:\/\//.test(value), {
      message: 'CTA URL must start with http:// or https://',
    }),
  sort: z.coerce.number().int(),
  isActive: z.boolean(),
  faqId: z.string().uuid('Select an FAQ.'),
  contentRefId: z.string().uuid().optional().or(z.literal('')),
});

export type FaqItemFormValues = z.infer<typeof faqItemFormSchema>;
