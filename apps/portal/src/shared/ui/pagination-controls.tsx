import { useMemo } from 'react';
import {
  ChevronLeft,
  ChevronRight,
  ChevronsLeft,
  ChevronsRight,
  LoaderCircle,
} from 'lucide-react';
import { cn } from '@/lib/utils';
import {
  Button,
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/ui';
import {
  clampPage,
  getVisiblePaginationPages,
  toPageCount,
} from '@/shared/lib/pagination';
import {
  Pagination,
  PaginationContent,
  PaginationEllipsis,
  PaginationItem,
} from '@/components/ui/pagination';

const DEFAULT_PAGE_SIZE_OPTIONS = [10, 20, 50, 100];

export function PaginationControls({
  page,
  pageSize,
  totalCount,
  isFetching,
  onPageChange,
  onPageSizeChange,
  pageSizeOptions = DEFAULT_PAGE_SIZE_OPTIONS,
}: {
  page: number;
  pageSize: number;
  totalCount: number;
  isFetching?: boolean;
  onPageChange: (page: number) => void;
  onPageSizeChange?: (pageSize: number) => void;
  pageSizeOptions?: number[];
}) {
  const pageCount = toPageCount(totalCount, pageSize);
  const safePage = clampPage(page, totalCount, pageSize);
  const start = totalCount === 0 ? 0 : (safePage - 1) * pageSize + 1;
  const end = Math.min(totalCount, safePage * pageSize);
  const resolvedPageSizeOptions = useMemo(
    () => [...new Set([...pageSizeOptions, pageSize])].sort((left, right) => left - right),
    [pageSize, pageSizeOptions],
  );
  const visiblePages = getVisiblePaginationPages(safePage, pageCount);
  const canGoBackward = safePage > 1;
  const canGoForward = safePage < pageCount;
  const summaryLabel = totalCount === 1 ? 'item' : 'items';
  const summary = totalCount === 0 ? '0 of 0 items' : `${start}-${end} of ${totalCount} ${summaryLabel}`;

  const pageSequence = visiblePages.flatMap((visiblePage, index) => {
    const previousPage = visiblePages[index - 1];
    const items: Array<number | string> = [];

    if (previousPage && visiblePage - previousPage > 1) {
      items.push(`ellipsis-${previousPage}-${visiblePage}`);
    }

    items.push(visiblePage);
    return items;
  });

  return (
    <div
      className="border-t border-border pt-4"
      aria-live="polite"
      aria-busy={isFetching}
    >
      <div className="flex items-center justify-between gap-3 overflow-x-auto pb-1 whitespace-nowrap">
        <div className="shrink-0 text-left text-sm font-medium text-foreground">
          {summary}
        </div>

        <div className="ml-auto flex min-w-max items-center justify-end gap-2">
          {onPageSizeChange ? (
            <Select
              value={String(pageSize)}
              onValueChange={(value) => onPageSizeChange(Number(value))}
            >
              <SelectTrigger className="h-7 w-[74px] shrink-0 px-2.5 text-xs" aria-label="Rows per page">
                <SelectValue placeholder={String(pageSize)} />
              </SelectTrigger>
              <SelectContent>
                {resolvedPageSizeOptions.map((option) => (
                  <SelectItem key={option} value={String(option)}>
                    {option}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          ) : null}

          {isFetching ? (
            <span className="inline-flex shrink-0 items-center text-muted-foreground" aria-hidden="true">
              <LoaderCircle className="size-3 animate-spin" />
            </span>
          ) : null}
          <Button
            type="button"
            variant="outline"
            size="sm"
            mode="icon"
            disabled={!canGoBackward}
            onClick={() => onPageChange(1)}
            aria-label="First page"
            title="First page"
          >
            <ChevronsLeft className="size-4" />
          </Button>
          <Button
            type="button"
            variant="outline"
            size="sm"
            mode="icon"
            disabled={!canGoBackward}
            onClick={() => onPageChange(safePage - 1)}
            aria-label="Previous page"
            title="Previous page"
          >
            <ChevronLeft className="size-4" />
          </Button>

          <Pagination className="mx-0 w-auto shrink-0 justify-start">
            <PaginationContent className="flex-nowrap justify-start">
              {pageSequence.map((item) => (
                <PaginationItem key={String(item)}>
                  {typeof item === 'number' ? (
                    <Button
                      type="button"
                      size="sm"
                      variant={safePage === item ? 'secondary' : 'outline'}
                      aria-current={safePage === item ? 'page' : undefined}
                      aria-label={`Page ${item}`}
                      className={cn(
                        'min-w-8 justify-center px-0 text-xs',
                        safePage === item && 'shadow-xs shadow-black/5',
                      )}
                      onClick={() => onPageChange(item)}
                    >
                      {item}
                    </Button>
                  ) : (
                    <PaginationEllipsis />
                  )}
                </PaginationItem>
              ))}
            </PaginationContent>
          </Pagination>

          <Button
            type="button"
            variant="outline"
            size="sm"
            mode="icon"
            disabled={!canGoForward}
            onClick={() => onPageChange(safePage + 1)}
            aria-label="Next page"
            title="Next page"
          >
            <ChevronRight className="size-4" />
          </Button>
          <Button
            type="button"
            variant="outline"
            size="sm"
            mode="icon"
            disabled={!canGoForward}
            onClick={() => onPageChange(pageCount)}
            aria-label="Last page"
            title="Last page"
          >
            <ChevronsRight className="size-4" />
          </Button>
        </div>
      </div>
    </div>
  );
}
