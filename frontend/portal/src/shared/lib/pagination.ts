export function toPagedQuery(page: number, pageSize: number, sorting?: string) {
  return {
    SkipCount: Math.max(0, (page - 1) * pageSize),
    MaxResultCount: pageSize,
    Sorting: sorting,
  };
}

export function toPageCount(totalCount: number, pageSize: number) {
  return Math.max(1, Math.ceil(totalCount / pageSize));
}
