export function toPagedQuery(
  page: number,
  pageSize: number,
  sorting?: string,
  filters?: Record<string, unknown>,
) {
  return {
    SkipCount: Math.max(0, (page - 1) * pageSize),
    MaxResultCount: pageSize,
    Sorting: sorting,
    ...filters,
  };
}

export function toPageCount(totalCount: number, pageSize: number) {
  return Math.max(1, Math.ceil(totalCount / pageSize));
}

export function clampPage(page: number, totalCount: number, pageSize: number) {
  if (!Number.isFinite(page)) {
    return 1;
  }

  return Math.min(Math.max(1, Math.floor(page)), toPageCount(totalCount, pageSize));
}

export function getVisiblePaginationPages(page: number, pageCount: number) {
  if (pageCount <= 7) {
    return Array.from({ length: pageCount }, (_, index) => index + 1);
  }

  const pages = new Set<number>([1, pageCount, page - 1, page, page + 1]);

  if (page <= 3) {
    pages.add(2);
    pages.add(3);
    pages.add(4);
  }

  if (page >= pageCount - 2) {
    pages.add(pageCount - 1);
    pages.add(pageCount - 2);
    pages.add(pageCount - 3);
  }

  return [...pages].filter((value) => value >= 1 && value <= pageCount).sort((left, right) => left - right);
}
