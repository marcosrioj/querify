import { useEffect, useMemo } from 'react';
import { Pencil, Plus, Trash2 } from 'lucide-react';
import { Link, useNavigate } from 'react-router-dom';
import { useContentRefList } from '@/domains/content-refs/hooks';
import { useFaqList } from '@/domains/faq/hooks';
import { useDeleteFaqItem, useFaqItemList } from '@/domains/faq-items/hooks';
import type { FaqItemDto } from '@/domains/faq-items/types';
import { ListLayout, PageHeader, SectionGrid } from '@/shared/layout/page-layouts';
import { clampPage } from '@/shared/lib/pagination';
import { useListQueryState } from '@/shared/lib/use-list-query-state';
import { DataTable, type DataTableColumn } from '@/shared/ui/data-table';
import { PaginationControls } from '@/shared/ui/pagination-controls';
import { EmptyState, ErrorState } from '@/shared/ui/placeholder-state';
import {
  Badge,
  Button,
  Input,
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/ui';

const sortingOptions = [
  { value: 'UpdatedDate DESC', label: 'Last updated' },
  { value: 'Question ASC', label: 'Question A-Z' },
  { value: 'VoteScore DESC', label: 'Vote score' },
  { value: 'AiConfidenceScore DESC', label: 'AI confidence' },
];

const FAQ_ITEM_FILTER_DEFAULTS = {
  active: 'all',
  faq: 'all',
} as const;

export function FaqItemListPage() {
  const navigate = useNavigate();
  const {
    debouncedSearch,
    filters,
    page,
    pageSize,
    search,
    setFilter,
    setPage,
    setPageSize,
    setSearch,
    setSorting,
    sorting,
  } = useListQueryState({
    defaultSorting: 'UpdatedDate DESC',
    filterDefaults: FAQ_ITEM_FILTER_DEFAULTS,
  });
  const activeFilter = filters.active;
  const faqFilter = filters.faq;
  const apiFaqId = faqFilter === 'all' ? undefined : faqFilter;
  const apiIsActive = activeFilter === 'all' ? undefined : activeFilter === 'true';

  const faqItemQuery = useFaqItemList({
    page,
    pageSize,
    sorting,
    searchText: debouncedSearch || undefined,
    faqId: apiFaqId,
    isActive: apiIsActive,
  });

  useEffect(() => {
    const totalCount = faqItemQuery.data?.totalCount;

    if (totalCount === undefined) {
      return;
    }

    const nextPage = clampPage(page, totalCount, pageSize);
    if (nextPage !== page) {
      setPage(nextPage, { replace: true });
    }
  }, [faqItemQuery.data?.totalCount, page, pageSize, setPage]);

  const faqOptionsQuery = useFaqList({ page: 1, pageSize: 100, sorting: 'Name ASC' });
  const contentRefQuery = useContentRefList({
    page: 1,
    pageSize: 100,
    sorting: 'Label ASC',
  });
  const deleteFaqItem = useDeleteFaqItem();
  const itemRows = faqItemQuery.data?.items ?? [];

  const faqLookup = useMemo(
    () =>
      Object.fromEntries((faqOptionsQuery.data?.items ?? []).map((faq) => [faq.id, faq.name])),
    [faqOptionsQuery.data?.items],
  );

  const contentRefLookup = useMemo(
    () =>
      Object.fromEntries(
        (contentRefQuery.data?.items ?? []).map((contentRef) => [
          contentRef.id,
          contentRef.label || contentRef.locator,
        ]),
      ),
    [contentRefQuery.data?.items],
  );
  const activeCount = itemRows.filter((item) => item.isActive).length;
  const sourcedCount = itemRows.filter((item) => item.contentRefId).length;
  const unsourcedCount = itemRows.filter((item) => !item.contentRefId).length;
  const activeFilterLabel =
    activeFilter === 'all' ? 'All states' : activeFilter === 'true' ? 'Active only' : 'Inactive only';
  const selectedFaqLabel = faqFilter === 'all' ? 'All FAQs' : faqLookup[faqFilter] ?? 'Selected FAQ';

  const columns: DataTableColumn<FaqItemDto>[] = [
    {
      key: 'question',
      header: 'FAQ item',
      cell: (item) => (
        <div className="space-y-1">
          <div className="font-medium text-mono">{item.question}</div>
          <div className="text-sm text-muted-foreground">{item.shortAnswer}</div>
        </div>
      ),
    },
    {
      key: 'faq',
      header: 'FAQ',
      cell: (item) => faqLookup[item.faqId] ?? item.faqId,
    },
    {
      key: 'status',
      header: 'Status',
      cell: (item) => (
        <Badge variant={item.isActive ? 'success' : 'mono'}>
          {item.isActive ? 'Active' : 'Inactive'}
        </Badge>
      ),
    },
    {
      key: 'contentRef',
      header: 'Content ref',
      cell: (item) =>
        item.contentRefId ? contentRefLookup[item.contentRefId] ?? item.contentRefId : 'None',
    },
    {
      key: 'actions',
      header: 'Actions',
      className: 'w-[140px]',
      cell: (item) => (
        <div
          className="flex items-center justify-end gap-1"
          onClick={(event) => event.stopPropagation()}
        >
          <Button asChild variant="ghost" mode="icon">
            <Link to={`/app/faq-items/${item.id}/edit`}>
              <Pencil className="size-4" />
            </Link>
          </Button>
          <Button
            variant="ghost"
            mode="icon"
            onClick={() => {
              if (window.confirm(`Delete FAQ item "${item.question}"?`)) {
                void deleteFaqItem.mutateAsync(item.id);
              }
            }}
          >
            <Trash2 className="size-4 text-destructive" />
          </Button>
        </div>
      ),
    },
  ];

  return (
    <ListLayout
      header={
        <PageHeader
          eyebrow="FAQ Items"
          title="FAQ Items"
          description="Manage answer records, link source material, and keep each FAQ ready for publishing."
          actions={
            <Button asChild>
              <Link to="/app/faq-items/new">
                <Plus className="size-4" />
                New FAQ item
              </Link>
            </Button>
          }
        />
      }
      filters={
        <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-[minmax(0,1fr)_220px_220px_220px]">
          <div className="sm:col-span-2 xl:col-span-1">
            <Input
              value={search}
              onChange={(event) => setSearch(event.target.value)}
              placeholder="Search FAQ items"
            />
          </div>
          <Select value={faqFilter} onValueChange={(value) => setFilter('faq', value)}>
            <SelectTrigger>
              <SelectValue placeholder="Filter by FAQ" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All FAQs</SelectItem>
              {(faqOptionsQuery.data?.items ?? []).map((faq) => (
                <SelectItem key={faq.id} value={faq.id}>
                  {faq.name}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
          <Select value={activeFilter} onValueChange={(value) => setFilter('active', value)}>
            <SelectTrigger>
              <SelectValue placeholder="Active state" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All states</SelectItem>
              <SelectItem value="true">Active</SelectItem>
              <SelectItem value="false">Inactive</SelectItem>
            </SelectContent>
          </Select>
          <Select value={sorting} onValueChange={setSorting}>
            <SelectTrigger>
              <SelectValue placeholder="Sort FAQ items" />
            </SelectTrigger>
            <SelectContent>
              {sortingOptions.map((option) => (
                <SelectItem key={option.value} value={option.value}>
                  {option.label}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
      }
    >
      <SectionGrid
        items={[
          {
            title: 'Answer records',
            value: faqItemQuery.data?.totalCount ?? 0,
            description: debouncedSearch ? `Search: ${debouncedSearch}` : selectedFaqLabel,
          },
          {
            title: 'Active on page',
            value: activeCount,
            description: activeFilterLabel,
          },
          {
            title: 'Linked sources',
            value: sourcedCount,
            description: sourcedCount ? 'Answers already grounded in content' : 'No sources linked in this slice',
          },
          {
            title: 'Missing sources',
            value: unsourcedCount,
            description: 'Candidates for enrichment',
          },
        ]}
      />
      <DataTable
        title="Answer catalog"
        description="Open an answer to refine copy, scoring, CTA, and source linkage."
        columns={columns}
        rows={itemRows}
        getRowId={(row) => row.id}
        loading={faqItemQuery.isLoading}
        onRowClick={(item) => navigate(`/app/faq-items/${item.id}`)}
        toolbar={
          <>
            <Badge variant="outline">{faqItemQuery.data?.totalCount ?? 0} total</Badge>
            <Badge variant={activeFilter === 'all' ? 'outline' : 'success'} appearance="outline">
              {activeFilterLabel}
            </Badge>
            {faqFilter !== 'all' ? (
              <Badge variant="info" appearance="outline">
                {selectedFaqLabel}
              </Badge>
            ) : null}
          </>
        }
        emptyState={
          <EmptyState
            title="No answers in view"
            description="Create FAQ items and connect them to the right knowledge space."
            action={{ label: 'Create FAQ item', to: '/app/faq-items/new' }}
          />
        }
        errorState={
          faqItemQuery.isError ? (
            <ErrorState
              title="Unable to load FAQ items"
              description="Refresh the answer catalog and try again."
              retry={() => void faqItemQuery.refetch()}
            />
          ) : undefined
        }
        footer={
          faqItemQuery.data ? (
            <PaginationControls
              page={page}
              pageSize={pageSize}
              totalCount={faqItemQuery.data.totalCount}
              onPageChange={setPage}
              onPageSizeChange={setPageSize}
              isFetching={faqItemQuery.isFetching}
            />
          ) : undefined
        }
      />
    </ListLayout>
  );
}
