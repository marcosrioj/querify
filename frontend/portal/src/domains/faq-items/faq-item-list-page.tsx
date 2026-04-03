import { Pencil, Plus, Trash2 } from 'lucide-react';
import { useDeferredValue, useEffect, useMemo, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useFaqList } from '@/domains/faq/hooks';
import { useContentRefList } from '@/domains/content-refs/hooks';
import { useDeleteFaqItem, useFaqItemList } from '@/domains/faq-items/hooks';
import type { FaqItemDto } from '@/domains/faq-items/types';
import { ListLayout, PageHeader } from '@/shared/layout/page-layouts';
import { DataTable, type DataTableColumn } from '@/shared/ui/data-table';
import { PaginationControls } from '@/shared/ui/pagination-controls';
import { EmptyState, ErrorState } from '@/shared/ui/placeholder-state';
import { Badge, Button, Input, Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/shared/ui';

const sortingOptions = [
  { value: 'UpdatedDate DESC', label: 'Last updated' },
  { value: 'Question ASC', label: 'Question A-Z' },
  { value: 'VoteScore DESC', label: 'Vote score' },
  { value: 'AiConfidenceScore DESC', label: 'AI confidence' },
];

export function FaqItemListPage() {
  const navigate = useNavigate();
  const [page, setPage] = useState(1);
  const [pageSize] = useState(12);
  const [sorting, setSorting] = useState('UpdatedDate DESC');
  const [search, setSearch] = useState('');
  const [activeFilter, setActiveFilter] = useState('all');
  const [faqFilter, setFaqFilter] = useState('all');
  const deferredSearch = useDeferredValue(search.trim());
  const apiFaqId = faqFilter === 'all' ? undefined : faqFilter;
  const apiIsActive =
    activeFilter === 'all' ? undefined : activeFilter === 'true';

  useEffect(() => {
    setPage(1);
  }, [apiFaqId, apiIsActive, deferredSearch, sorting]);

  const faqItemQuery = useFaqItemList({
    page,
    pageSize,
    sorting,
    searchText: deferredSearch || undefined,
    faqId: apiFaqId,
    isActive: apiIsActive,
  });
  const faqOptionsQuery = useFaqList({ page: 1, pageSize: 100, sorting: 'Name ASC' });
  const contentRefQuery = useContentRefList({
    page: 1,
    pageSize: 100,
    sorting: 'Label ASC',
  });
  const deleteFaqItem = useDeleteFaqItem();

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
          description="Backed by the real FAQ Item Portal CRUD endpoints. Search, filters, sorting, and paging are API-driven."
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
        <div className="grid gap-3 md:grid-cols-[minmax(0,1fr)_220px_220px_220px]">
          <Input
            value={search}
            onChange={(event) => setSearch(event.target.value)}
            placeholder="Search FAQ items"
          />
          <Select value={faqFilter} onValueChange={setFaqFilter}>
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
          <Select value={activeFilter} onValueChange={setActiveFilter}>
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
      <DataTable
        title="Answer catalog"
        columns={columns}
        rows={faqItemQuery.data?.items ?? []}
        getRowId={(row) => row.id}
        loading={faqItemQuery.isLoading}
        onRowClick={(item) => navigate(`/app/faq-items/${item.id}`)}
        emptyState={
          <EmptyState
            title="No FAQ items on this page"
            description="Create answer records and associate them with FAQs."
            action={{ label: 'Create FAQ item', to: '/app/faq-items/new' }}
          />
        }
        errorState={
          faqItemQuery.isError ? (
            <ErrorState
              title="Unable to load FAQ items"
              description="The FAQ Item Portal API request failed."
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
            />
          ) : undefined
        }
      />
    </ListLayout>
  );
}
