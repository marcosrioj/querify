import { useState } from 'react';

export function useCopyToClipboard(
  { timeout = 2000 }: { timeout?: number } = {},
) {
  const [copied, setCopied] = useState(false);

  const copy = (value: string) => {
    if (typeof window === 'undefined' || !navigator.clipboard.writeText || !value) {
      return;
    }

    navigator.clipboard.writeText(value).then(
      () => {
        setCopied(true);
        window.setTimeout(() => {
          setCopied(false);
        }, timeout);
      },
      console.error,
    );
  };

  return { copy, copied };
}
