import { PropsWithChildren, useState } from 'react';
import {
  MutationCache,
  QueryCache,
  QueryClient,
  QueryClientProvider,
} from '@tanstack/react-query';
import { toast } from 'sonner';
import { isAbortError, toErrorMessage } from '@/platform/api/api-error';
import { logger } from '@/platform/telemetry/logger';

export function QueryProvider({ children }: PropsWithChildren) {
  const [queryClient] = useState(
    () =>
      new QueryClient({
        defaultOptions: {
          queries: {
            retry: 1,
            refetchOnWindowFocus: false,
            staleTime: 30_000,
          },
          mutations: {
            retry: 0,
          },
        },
        queryCache: new QueryCache({
          onError: (error, query) => {
            if (isAbortError(error)) {
              return;
            }

            logger.error('Query failed', {
              queryKey: query.queryKey,
              error,
            });
            toast.error(toErrorMessage(error));
          },
        }),
        mutationCache: new MutationCache({
          onError: (error, _variables, _context, mutation) => {
            if (isAbortError(error)) {
              return;
            }

            logger.error('Mutation failed', {
              mutationKey: mutation.options.mutationKey,
              error,
            });
            toast.error(toErrorMessage(error));
          },
        }),
      }),
  );

  return (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );
}
