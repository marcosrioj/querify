import { useCallback, useMemo, useState } from "react";
import { useDebouncedValue } from "@/shared/lib/use-debounced-value";

export const RELATIONSHIP_PAGE_SIZE_OPTIONS = [5, 10, 20] as const;

export function useRelationshipListState<
  TFilters extends Record<string, string>,
>({
  defaultSorting,
  filterDefaults,
  searchDebounceMs = 350,
}: {
  defaultSorting: string;
  filterDefaults: TFilters;
  searchDebounceMs?: number;
}) {
  const [page, setPage] = useState(1);
  const [pageSize, setPageSizeState] = useState(
    RELATIONSHIP_PAGE_SIZE_OPTIONS[0],
  );
  const [sorting, setSortingState] = useState(defaultSorting);
  const [search, setSearchState] = useState("");
  const [filters, setFiltersState] = useState<TFilters>(filterDefaults);
  const debouncedSearch = useDebouncedValue(search.trim(), searchDebounceMs);

  const setPageSize = useCallback((nextPageSize: number) => {
    const resolvedPageSize = RELATIONSHIP_PAGE_SIZE_OPTIONS.includes(
      nextPageSize as (typeof RELATIONSHIP_PAGE_SIZE_OPTIONS)[number],
    )
      ? nextPageSize
      : RELATIONSHIP_PAGE_SIZE_OPTIONS[0];

    setPageSizeState(resolvedPageSize);
    setPage(1);
  }, []);

  const setSorting = useCallback((nextSorting: string) => {
    setSortingState(nextSorting);
    setPage(1);
  }, []);

  const setSearch = useCallback((nextSearch: string) => {
    setSearchState(nextSearch);
    setPage(1);
  }, []);

  const setFilter = useCallback(
    <K extends keyof TFilters>(key: K, value: TFilters[K]) => {
      setFiltersState((current) => ({
        ...current,
        [key]: value,
      }));
      setPage(1);
    },
    [],
  );

  const resetFilters = useCallback(() => {
    setSearchState("");
    setFiltersState(filterDefaults);
    setSortingState(defaultSorting);
    setPage(1);
  }, [defaultSorting, filterDefaults]);

  const hasSearch = search.trim().length > 0;
  const activeFilterCount = useMemo(
    () =>
      [
        hasSearch,
        ...Object.entries(filters).map(
          ([key, value]) => value !== filterDefaults[key as keyof TFilters],
        ),
      ].filter(Boolean).length,
    [filters, filterDefaults, hasSearch],
  );

  return {
    activeFilterCount,
    debouncedSearch,
    filters,
    page,
    pageSize,
    resetFilters,
    search,
    setFilter,
    setPage,
    setPageSize,
    setSearch,
    setSorting,
    sorting,
  };
}
