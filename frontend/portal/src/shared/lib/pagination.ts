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
