import { z } from 'zod';
import { SourceKind, VisibilityScope } from '@/shared/constants/backend-enums';
import { numericEnumSchema } from '@/shared/lib/zod';

export const sourceFormSchema = z.object({
  kind: numericEnumSchema(SourceKind),
  locator: z.string().min(3, 'Locator is required.'),
  label: z.string().max(200, 'Keep the label concise.').optional(),
  contextNote: z.string().max(200, 'Keep the context note concise.').optional(),
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
  metadataJson: z.string().max(4000, 'Keep the metadata concise.').optional(),
  visibility: numericEnumSchema(VisibilityScope),
  allowsCitation: z.boolean(),
  capturedAtUtc: z.string().max(64, 'Use an ISO timestamp.').optional(),
  markVerified: z.boolean(),
});

export type SourceFormValues = z.infer<typeof sourceFormSchema>;
