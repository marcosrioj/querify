import { z } from 'zod';

export function numericEnumSchema<T extends Record<string, number | string>>(
  source: T,
  message = 'Invalid selection.',
) {
  const values = Object.values(source).filter(
    (value): value is number => typeof value === 'number',
  );

  return z
    .coerce.number()
    .refine((value) => values.includes(value), message);
}
