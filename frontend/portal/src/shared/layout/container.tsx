import { PropsWithChildren } from 'react';
import { cn } from '@/lib/utils';

export function Container({
  children,
  className,
}: PropsWithChildren<{ className?: string }>) {
  return (
    <div className={cn('mx-auto w-full max-w-[1320px] px-4 lg:px-6', className)}>
      {children}
    </div>
  );
}
