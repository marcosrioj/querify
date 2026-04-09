import type { ReactNode } from 'react';
import { translateText } from '@/shared/lib/i18n-core';
import { translateRenderableNode } from '@/shared/lib/translate-renderable-node';

export function translateNode(node: ReactNode) {
  return translateRenderableNode(node);
}

export function translateMaybeString(
  value: ReactNode,
  t: typeof translateText = translateText,
) {
  if (typeof value === 'string') {
    return t(value);
  }

  return translateRenderableNode(value);
}
