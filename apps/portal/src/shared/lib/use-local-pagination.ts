import { useCallback, useEffect, useMemo, useState } from 'react';
import { clampPage } from '@/shared/lib/pagination';

export function useLocalPagination<T>({
  items,
  defaultPageSize = 5,
}: {
  items: T[];
  defaultPageSize?: number;
}) {
  const [page, setPage] = useState(1);
  const [pageSize, setPageSizeState] = useState(defaultPageSize);
  const totalCount = items.length;

  useEffect(() => {
    const nextPage = clampPage(page, totalCount, pageSize);

    if (nextPage !== page) {
      setPage(nextPage);
    }
  }, [page, pageSize, totalCount]);

  const pagedItems = useMemo(() => {
    const startIndex = (page - 1) * pageSize;
    return items.slice(startIndex, startIndex + pageSize);
  }, [items, page, pageSize]);

  const setPageSize = useCallback((nextPageSize: number) => {
    setPageSizeState(nextPageSize);
    setPage(1);
  }, []);

  return {
    page,
    pageSize,
    pagedItems,
    setPage,
    setPageSize,
    totalCount,
  };
}
