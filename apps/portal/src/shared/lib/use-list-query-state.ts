import {
  startTransition,
  useCallback,
  useEffect,
  useMemo,
  useState,
} from "react";
import { useSearchParams } from "react-router-dom";
import { useDebouncedValue } from "@/shared/lib/use-debounced-value";

type NavigationOptions = {
  replace?: boolean;
};

type UseListQueryStateOptions<TFilters extends Record<string, string>> = {
  allowedPageSizes?: number[];
  defaultPageSize?: number;
  defaultSorting: string;
  filterDefaults?: TFilters;
  searchDebounceMs?: number;
};

const DEFAULT_MAIN_PAGE_SIZE_OPTIONS = [10, 20, 50, 100];

function parsePositiveInteger(value: string | null, fallback: number) {
  const parsedValue = Number(value);

  if (!Number.isFinite(parsedValue) || parsedValue < 1) {
    return fallback;
  }

  return Math.floor(parsedValue);
}

function normalizeSearchValue(value: string) {
  return value.trim();
}

function resolvePageSize(
  value: string | null,
  fallback: number,
  allowedPageSizes: number[],
) {
  const parsedValue = parsePositiveInteger(value, fallback);

  return allowedPageSizes.includes(parsedValue) ? parsedValue : fallback;
}

export function useListQueryState<TFilters extends Record<string, string>>({
  allowedPageSizes = DEFAULT_MAIN_PAGE_SIZE_OPTIONS,
  defaultPageSize = 10,
  defaultSorting,
  filterDefaults,
  searchDebounceMs = 350,
}: UseListQueryStateOptions<TFilters>) {
  const [searchParams, setSearchParams] = useSearchParams();
  const resolvedFilterDefaults = useMemo(
    () => filterDefaults ?? ({} as TFilters),
    [filterDefaults],
  );
  const page = parsePositiveInteger(searchParams.get("page"), 1);
  const rawPageSize = searchParams.get("pageSize");
  const pageSize = resolvePageSize(
    rawPageSize,
    defaultPageSize,
    allowedPageSizes,
  );
  const sorting = searchParams.get("sort") ?? defaultSorting;
  const searchFromUrl = searchParams.get("search") ?? "";
  const [search, setSearch] = useState(searchFromUrl);
  const normalizedSearch = normalizeSearchValue(search);
  const debouncedSearch = useDebouncedValue(normalizedSearch, searchDebounceMs);

  const filters = useMemo(() => {
    return Object.fromEntries(
      Object.entries(resolvedFilterDefaults).map(([key, fallbackValue]) => [
        key,
        searchParams.get(key) ?? fallbackValue,
      ]),
    ) as TFilters;
  }, [resolvedFilterDefaults, searchParams]);

  useEffect(() => {
    setSearch(searchFromUrl);
  }, [searchFromUrl]);

  const compactSearchParams = useCallback(
    (nextSearchParams: URLSearchParams) => {
      const normalizedPage = parsePositiveInteger(
        nextSearchParams.get("page"),
        1,
      );
      const normalizedPageSize = resolvePageSize(
        nextSearchParams.get("pageSize"),
        defaultPageSize,
        allowedPageSizes,
      );
      const normalizedSort = nextSearchParams.get("sort")?.trim() ?? "";
      const normalizedSearch = normalizeSearchValue(
        nextSearchParams.get("search") ?? "",
      );

      if (normalizedPage <= 1) {
        nextSearchParams.delete("page");
      } else {
        nextSearchParams.set("page", String(normalizedPage));
      }

      if (normalizedPageSize === defaultPageSize) {
        nextSearchParams.delete("pageSize");
      } else {
        nextSearchParams.set("pageSize", String(normalizedPageSize));
      }

      if (!normalizedSort || normalizedSort === defaultSorting) {
        nextSearchParams.delete("sort");
      } else {
        nextSearchParams.set("sort", normalizedSort);
      }

      if (!normalizedSearch) {
        nextSearchParams.delete("search");
      } else {
        nextSearchParams.set("search", normalizedSearch);
      }

      Object.entries(resolvedFilterDefaults).forEach(([key, fallbackValue]) => {
        const nextValue = nextSearchParams.get(key);

        if (!nextValue || nextValue === fallbackValue) {
          nextSearchParams.delete(key);
          return;
        }

        nextSearchParams.set(key, nextValue);
      });
    },
    [allowedPageSizes, defaultPageSize, defaultSorting, resolvedFilterDefaults],
  );

  const updateSearchParams = useCallback(
    (
      update: (nextSearchParams: URLSearchParams) => void,
      { replace = false }: NavigationOptions = {},
    ) => {
      const nextSearchParams = new URLSearchParams(searchParams);
      update(nextSearchParams);
      compactSearchParams(nextSearchParams);

      startTransition(() => {
        setSearchParams(nextSearchParams, { replace });
      });
    },
    [compactSearchParams, searchParams, setSearchParams],
  );

  useEffect(() => {
    if (debouncedSearch !== normalizedSearch) {
      return;
    }

    if (debouncedSearch === searchFromUrl) {
      return;
    }

    updateSearchParams(
      (nextSearchParams) => {
        if (debouncedSearch) {
          nextSearchParams.set("search", debouncedSearch);
        } else {
          nextSearchParams.delete("search");
        }

        nextSearchParams.delete("page");
      },
      { replace: true },
    );
  }, [debouncedSearch, normalizedSearch, searchFromUrl, updateSearchParams]);

  useEffect(() => {
    if (!rawPageSize) {
      return;
    }

    if (rawPageSize === String(pageSize)) {
      return;
    }

    updateSearchParams(
      (nextSearchParams) => {
        nextSearchParams.set("pageSize", String(pageSize));
        nextSearchParams.delete("page");
      },
      { replace: true },
    );
  }, [pageSize, rawPageSize, updateSearchParams]);

  const setPage = useCallback(
    (nextPage: number, options?: NavigationOptions) => {
      updateSearchParams((nextSearchParams) => {
        nextSearchParams.set(
          "page",
          String(parsePositiveInteger(String(nextPage), 1)),
        );
      }, options);
    },
    [updateSearchParams],
  );

  const setPageSize = useCallback(
    (nextPageSize: number, options: NavigationOptions = { replace: true }) => {
      updateSearchParams((nextSearchParams) => {
        nextSearchParams.set(
          "pageSize",
          String(
            resolvePageSize(
              String(nextPageSize),
              defaultPageSize,
              allowedPageSizes,
            ),
          ),
        );
        nextSearchParams.delete("page");
      }, options);
    },
    [allowedPageSizes, defaultPageSize, updateSearchParams],
  );

  const setSorting = useCallback(
    (nextSorting: string, options: NavigationOptions = { replace: true }) => {
      updateSearchParams((nextSearchParams) => {
        nextSearchParams.set("sort", nextSorting);
        nextSearchParams.delete("page");
      }, options);
    },
    [updateSearchParams],
  );

  const setFilter = useCallback(
    <K extends keyof TFilters>(
      key: K,
      value: TFilters[K],
      options: NavigationOptions = { replace: true },
    ) => {
      updateSearchParams((nextSearchParams) => {
        if (!value || value === resolvedFilterDefaults[key]) {
          nextSearchParams.delete(String(key));
        } else {
          nextSearchParams.set(String(key), value);
        }

        nextSearchParams.delete("page");
      }, options);
    },
    [resolvedFilterDefaults, updateSearchParams],
  );

  const setFilters = useCallback(
    (
      nextFilters: Partial<TFilters>,
      options: NavigationOptions = { replace: true },
    ) => {
      updateSearchParams((nextSearchParams) => {
        (
          Object.entries(nextFilters) as Array<
            [keyof TFilters, TFilters[keyof TFilters] | undefined]
          >
        ).forEach(([key, value]) => {
          if (!value || value === resolvedFilterDefaults[key]) {
            nextSearchParams.delete(String(key));
          } else {
            nextSearchParams.set(String(key), value);
          }
        });

        nextSearchParams.delete("page");
      }, options);
    },
    [resolvedFilterDefaults, updateSearchParams],
  );

  const resetFilters = useCallback(
    (options: NavigationOptions = { replace: true }) => {
      setSearch("");

      updateSearchParams((nextSearchParams) => {
        nextSearchParams.delete("search");

        Object.keys(resolvedFilterDefaults).forEach((key) => {
          nextSearchParams.delete(key);
        });

        nextSearchParams.delete("page");
      }, options);
    },
    [resolvedFilterDefaults, updateSearchParams],
  );

  return {
    debouncedSearch,
    filters,
    page,
    pageSize,
    search,
    setFilter,
    setFilters,
    setPage,
    setPageSize,
    resetFilters,
    setSearch,
    setSorting,
    sorting,
  };
}
