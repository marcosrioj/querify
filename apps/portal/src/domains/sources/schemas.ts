import { z } from 'zod';
import { SourceKind, VisibilityScope } from '@/shared/constants/backend-enums';
import { numericEnumSchema } from '@/shared/lib/zod';

export const sourceFormSchema = z.object({
  kind: numericEnumSchema(SourceKind),
  locator: z.string().min(3, 'Locator is required.'),
  label: z.string().max(200, 'Keep the label concise.').optional(),
  contextNote: z.string().max(2000, 'Keep the context note within 2,000 characters.').optional(),
  externalId: z.string().max(120, 'Keep the external id concise.').optional(),
  language: z
    .string()
    .min(2, 'Language is required.')
    .max(16, 'Keep the language code concise.'),
  mediaType: z.string().max(120, 'Keep the media type concise.').optional(),
  checksum: z
    .string()
    .max(128, 'Keep the checksum within the backend limit.')
    .optional(),
  metadataJson: z
    .string()
    .max(4000, 'Keep the metadata concise.')
    .optional()
    .refine((value) => {
      if (!value?.trim()) {
        return true;
      }

      try {
        JSON.parse(value);
        return true;
      } catch {
        return false;
      }
    }, 'Enter valid JSON.'),
  visibility: numericEnumSchema(VisibilityScope),
  markVerified: z.boolean(),
});

export type SourceFormValues = z.infer<typeof sourceFormSchema>;
