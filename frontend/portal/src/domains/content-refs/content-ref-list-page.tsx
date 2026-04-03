import { Pencil, Plus, Trash2 } from 'lucide-react';
import { useMemo, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useDeleteContentRef, useContentRefList } from '@/domains/content-refs/hooks';
import type { ContentRefDto } from '@/domains/content-refs/types';
import {
  ContentRefKind,
  contentRefKindLabels,
} from '@/shared/constants/backend-enums';
import { ListLayout, PageHeader } from '@/shared/layout/page-layouts';
import { DataTable, type DataTableColumn } from '@/shared/ui/data-table';
import { PaginationControls } from '@/shared/ui/pagination-controls';
import { EmptyState, ErrorState } from '@/shared/ui/placeholder-state';
import { ContentRefKindBadge } from '@/shared/ui/status-badges';
import { Button, Input, Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/shared/ui';

const sortingOptions = [
  { value: 'UpdatedDate DESC', label: 'Last updated' },
  { value: 'Label ASC', label: 'Label A-Z' },
  { value: 'Kind ASC', label: 'Kind' },
  { value: 'Locator ASC', label: 'Locator' },
];

export function ContentRefListPage() {
  const navigate = useNavigate();
  const [page, setPage] = useState(1);
  const [pageSize] = useState(12);
  const [sorting, setSorting] = useState('UpdatedDate DESC');
  const [search, setSearch] = useState('');
  const [kindFilter, setKindFilter] = useState('all');

  const contentRefQuery = useContentRefList({ page, pageSize, sorting });
  const deleteContentRef = useDeleteContentRef();

  const rows = useMemo(() => {
    return (contentRefQuery.data?.items ?? []).filter((contentRef) => {
      const matchesSearch =
        contentRef.locator.toLowerCase().includes(search.toLowerCase()) ||
        (contentRef.label ?? '').toLowerCase().includes(search.toLowerCase());
      const matchesKind =
        kindFilter === 'all' || contentRef.kind === Number(kindFilter);
      return matchesSearch && matchesKind;
    });
  }, [contentRefQuery.data?.items, kindFilter, search]);

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
          <div className="max-w-[360px] truncate text-sm text-muted-foreground">
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
          description="Backed by the real Portal content ref endpoints. Search filters are client-side on the loaded page until the backend adds filter contracts."
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
        <div className="grid gap-3 md:grid-cols-[minmax(0,1fr)_220px_220px]">
          <Input
            value={search}
            onChange={(event) => setSearch(event.target.value)}
            placeholder="Search loaded content refs"
          />
          <Select value={kindFilter} onValueChange={setKindFilter}>
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
      <DataTable
        title="Source material registry"
        columns={columns}
        rows={rows}
        getRowId={(row) => row.id}
        loading={contentRefQuery.isLoading}
        onRowClick={(contentRef) => navigate(`/app/content-refs/${contentRef.id}`)}
        emptyState={
          <EmptyState
            title="No content refs on this page"
            description="Create content references to build reusable source material for FAQ generation and curation."
            action={{ label: 'Create content ref', to: '/app/content-refs/new' }}
          />
        }
        errorState={
          contentRefQuery.isError ? (
            <ErrorState
              title="Unable to load content refs"
              description="The Content Ref Portal API request failed."
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
            />
          ) : undefined
        }
      />
    </ListLayout>
  );
}
