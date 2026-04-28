import { PaginationControls } from "@/shared/ui/pagination-controls";

const CHILD_LIST_PAGE_SIZE_OPTIONS = [5, 10, 20];

export function ChildListPagination({
  page,
  pageSize,
  totalCount,
  isFetching,
  onPageChange,
  onPageSizeChange,
}: {
  page: number;
  pageSize: number;
  totalCount: number;
  isFetching?: boolean;
  onPageChange: (page: number) => void;
  onPageSizeChange: (pageSize: number) => void;
}) {
  if (totalCount <= CHILD_LIST_PAGE_SIZE_OPTIONS[0]) {
    return null;
  }

  return (
    <PaginationControls
      page={page}
      pageSize={pageSize}
      totalCount={totalCount}
      isFetching={isFetching}
      onPageChange={onPageChange}
      onPageSizeChange={onPageSizeChange}
      pageSizeOptions={CHILD_LIST_PAGE_SIZE_OPTIONS}
    />
  );
}
