import { useEffect } from 'react';
import { Pencil, Plus, Trash2 } from 'lucide-react';
import { Link, useNavigate } from 'react-router-dom';
import { useContentRefList, useDeleteContentRef } from '@/domains/content-refs/hooks';
import type { ContentRefDto } from '@/domains/content-refs/types';
import {
  ContentRefKind,
  contentRefKindLabels,
} from '@/shared/constants/backend-enums';
import { ListLayout, PageHeader, SectionGrid } from '@/shared/layout/page-layouts';
import { clampPage } from '@/shared/lib/pagination';
import { useListQueryState } from '@/shared/lib/use-list-query-state';
import { DataTable, type DataTableColumn } from '@/shared/ui/data-table';
import { PaginationControls } from '@/shared/ui/pagination-controls';
import { EmptyState, ErrorState } from '@/shared/ui/placeholder-state';
import { ContentRefKindBadge } from '@/shared/ui/status-badges';
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
  { value: 'Label ASC', label: 'Label A-Z' },
  { value: 'Kind ASC', label: 'Kind' },
  { value: 'Locator ASC', label: 'Locator' },
];

const CONTENT_REF_FILTER_DEFAULTS = {
  kind: 'all',
} as const;

export function ContentRefListPage() {
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
    filterDefaults: CONTENT_REF_FILTER_DEFAULTS,
  });
  const kindFilter = filters.kind;
  const apiKind = kindFilter === 'all' ? undefined : Number(kindFilter);

  const contentRefQuery = useContentRefList({
    page,
    pageSize,
    sorting,
    searchText: debouncedSearch || undefined,
    kind: apiKind,
  });

  useEffect(() => {
    const totalCount = contentRefQuery.data?.totalCount;

    if (totalCount === undefined) {
      return;
    }

    const nextPage = clampPage(page, totalCount, pageSize);
    if (nextPage !== page) {
      setPage(nextPage, { replace: true });
    }
  }, [contentRefQuery.data?.totalCount, page, pageSize, setPage]);

  const deleteContentRef = useDeleteContentRef();
  const contentRefRows = contentRefQuery.data?.items ?? [];
  const scopedCount = contentRefRows.filter((contentRef) => Boolean(contentRef.scope)).length;
  const unlabeledCount = contentRefRows.filter((contentRef) => !contentRef.label).length;
  const selectedKindLabel =
    kindFilter === 'all' ? 'All kinds' : contentRefKindLabels[Number(kindFilter) as ContentRefKind];
  const sortingLabel = sortingOptions.find((option) => option.value === sorting)?.label ?? 'Custom';

  const columns: DataTableColumn<ContentRefDto>[] = [
    {
      key: 'kind',
      header: 'Kind',
      cell: (contentRef) => <ContentRefKindBadge kind={contentRef.kind} />,
    },
    {
      key: 'locator',
      header: 'Locator',
      cell: (contentRef) => (
        <div className="space-y-1">
          <div className="font-medium text-mono">
            {contentRef.label || 'Untitled content ref'}
          </div>
          <div className="break-all text-sm text-muted-foreground lg:max-w-[360px] lg:truncate lg:break-normal">
            {contentRef.locator}
          </div>
        </div>
      ),
    },
    {
      key: 'scope',
      header: 'Scope',
      cell: (contentRef) => contentRef.scope || 'No scope',
    },
    {
      key: 'actions',
      header: 'Actions',
      className: 'w-[140px]',
      cell: (contentRef) => (
        <div
          className="flex items-center justify-end gap-1"
          onClick={(event) => event.stopPropagation()}
        >
          <Button asChild variant="ghost" mode="icon">
            <Link to={`/app/content-refs/${contentRef.id}/edit`}>
              <Pencil className="size-4" />
            </Link>
          </Button>
          <Button
            variant="ghost"
            mode="icon"
            onClick={() => {
              if (
                window.confirm(
                  `Delete content ref "${contentRef.label || contentRef.locator}"?`,
                )
              ) {
                void deleteContentRef.mutateAsync(contentRef.id);
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
          eyebrow="Content Refs"
          title="Content references"
          description="Track reusable source material for answers, generation, and future curation work."
          actions={
            <Button asChild>
              <Link to="/app/content-refs/new">
                <Plus className="size-4" />
                New content ref
              </Link>
            </Button>
          }
        />
      }
      filters={
        <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-[minmax(0,1fr)_220px_220px]">
          <div className="sm:col-span-2 xl:col-span-1">
            <Input
              value={search}
              onChange={(event) => setSearch(event.target.value)}
              placeholder="Search content refs"
            />
          </div>
          <Select value={kindFilter} onValueChange={(value) => setFilter('kind', value)}>
            <SelectTrigger>
              <SelectValue placeholder="Filter by kind" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All kinds</SelectItem>
              {Object.entries(contentRefKindLabels).map(([value, label]) => (
                <SelectItem key={value} value={value}>
                  {label}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
          <Select value={sorting} onValueChange={setSorting}>
            <SelectTrigger>
              <SelectValue placeholder="Sort content refs" />
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
            title: 'Source catalog',
            value: contentRefQuery.data?.totalCount ?? 0,
            description: debouncedSearch ? `Search: ${debouncedSearch}` : selectedKindLabel,
          },
          {
            title: 'Scoped sources',
            value: scopedCount,
            description: scopedCount ? 'Grouped for cleaner reuse' : 'No scope labels in this slice',
          },
          {
            title: 'Untitled sources',
            value: unlabeledCount,
            description: unlabeledCount ? 'Good candidates for cleanup' : 'Labels are in good shape',
          },
          {
            title: 'View order',
            value: sortingLabel,
            description: 'Current catalog sort',
          },
        ]}
      />
      <DataTable
        title="Source material registry"
        description="Open a content ref to see where it is reused across FAQs and answers."
        columns={columns}
        rows={contentRefRows}
        getRowId={(row) => row.id}
        loading={contentRefQuery.isLoading}
        onRowClick={(contentRef) => navigate(`/app/content-refs/${contentRef.id}`)}
        toolbar={
          <>
            <Badge variant="outline">{contentRefQuery.data?.totalCount ?? 0} total</Badge>
            <Badge variant={kindFilter === 'all' ? 'outline' : 'info'} appearance="outline">
              {selectedKindLabel}
            </Badge>
          </>
        }
        emptyState={
          <EmptyState
            title="No source material in view"
            description="Create content refs to ground answer generation and editorial review."
            action={{ label: 'Create content ref', to: '/app/content-refs/new' }}
          />
        }
        errorState={
          contentRefQuery.isError ? (
            <ErrorState
              title="Unable to load content refs"
              description="Refresh the source catalog and try again."
              retry={() => void contentRefQuery.refetch()}
            />
          ) : undefined
        }
        footer={
          contentRefQuery.data ? (
            <PaginationControls
              page={page}
              pageSize={pageSize}
              totalCount={contentRefQuery.data.totalCount}
              onPageChange={setPage}
              onPageSizeChange={setPageSize}
              isFetching={contentRefQuery.isFetching}
            />
          ) : undefined
        }
      />
    </ListLayout>
  );
}
