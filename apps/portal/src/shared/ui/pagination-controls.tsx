import { ChevronLeft, ChevronRight } from 'lucide-react';
import { Button } from '@/shared/ui';
import { toPageCount } from '@/shared/lib/pagination';

export function PaginationControls({
  page,
  pageSize,
  totalCount,
  onPageChange,
}: {
  page: number;
  pageSize: number;
  totalCount: number;
  onPageChange: (page: number) => void;
}) {
  const pageCount = toPageCount(totalCount, pageSize);
  const start = totalCount === 0 ? 0 : (page - 1) * pageSize + 1;
  const end = Math.min(totalCount, page * pageSize);

  return (
    <div className="flex flex-col gap-3 border-t border-border pt-4 text-sm text-muted-foreground md:flex-row md:items-center md:justify-between">
      <p>
        Showing <span className="font-medium text-foreground">{start}</span>-
        <span className="font-medium text-foreground">{end}</span> of{' '}
        <span className="font-medium text-foreground">{totalCount}</span>
      </p>
      <div className="grid grid-cols-[1fr_auto_1fr] items-center gap-2 sm:flex sm:flex-wrap sm:items-center">
        <Button
          variant="outline"
          size="sm"
          className="justify-center"
          disabled={page <= 1}
          onClick={() => onPageChange(page - 1)}
        >
          <ChevronLeft className="size-4" />
          <span className="hidden sm:inline">Previous</span>
        </Button>
        <span className="min-w-0 text-center text-sm sm:min-w-[96px]">
          Page {page} / {pageCount}
        </span>
        <Button
          variant="outline"
          size="sm"
          className="justify-center"
          disabled={page >= pageCount}
          onClick={() => onPageChange(page + 1)}
        >
          <span className="hidden sm:inline">Next</span>
          <ChevronRight className="size-4" />
        </Button>
      </div>
    </div>
  );
}
