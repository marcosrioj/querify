import { z } from 'zod';

export const tagFormSchema = z.object({
  name: z.string().min(2, 'Tag name is required.').max(100, 'Keep the tag concise.'),
});

export type TagFormValues = z.infer<typeof tagFormSchema>;
