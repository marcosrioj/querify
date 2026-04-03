import { Pencil, Plus, Trash2, WandSparkles } from 'lucide-react';
import { Link, useNavigate } from 'react-router-dom';
import { useDeferredValue, useEffect, useState } from 'react';
import { useDeleteFaq, useFaqList, useRequestFaqGeneration } from '@/domains/faq/hooks';
import { faqStatusLabels, FaqStatus } from '@/shared/constants/backend-enums';
import { FaqDto } from '@/domains/faq/types';
import { ListLayout, PageHeader, SectionGrid } from '@/shared/layout/page-layouts';
import { DataTable, type DataTableColumn } from '@/shared/ui/data-table';
import { PaginationControls } from '@/shared/ui/pagination-controls';
import { EmptyState, ErrorState } from '@/shared/ui/placeholder-state';
import { FaqStatusBadge, SortStrategyBadge } from '@/shared/ui/status-badges';
import { Badge, Button, Input, Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/shared/ui';

const sortingOptions = [
  { value: 'UpdatedDate DESC', label: 'Last updated' },
  { value: 'Name ASC', label: 'Name A-Z' },
  { value: 'Language ASC', label: 'Language' },
  { value: 'Status ASC', label: 'Status' },
];

export function FaqListPage() {
  const navigate = useNavigate();
  const [page, setPage] = useState(1);
  const [pageSize] = useState(12);
  const [sorting, setSorting] = useState('UpdatedDate DESC');
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('all');
  const deferredSearch = useDeferredValue(search.trim());
  const apiStatus = statusFilter === 'all' ? undefined : Number(statusFilter);

  useEffect(() => {
    setPage(1);
  }, [apiStatus, deferredSearch, sorting]);

  const faqQuery = useFaqList({
    page,
    pageSize,
    sorting,
    searchText: deferredSearch || undefined,
    status: apiStatus,
  });
  const deleteFaq = useDeleteFaq();
  const requestGeneration = useRequestFaqGeneration();
  const faqRows = faqQuery.data?.items ?? [];
  const publishedCount = faqRows.filter((faq) => faq.status === FaqStatus.Published).length;
  const draftCount = faqRows.filter((faq) => faq.status === FaqStatus.Draft).length;
  const ctaEnabledCount = faqRows.filter((faq) => faq.ctaEnabled).length;
  const sortingLabel = sortingOptions.find((option) => option.value === sorting)?.label ?? 'Custom';
  const activeStatusLabel =
    statusFilter === 'all' ? 'All statuses' : faqStatusLabels[Number(statusFilter) as FaqStatus];

  const columns: DataTableColumn<FaqDto>[] = [
    {
      key: 'name',
      header: 'FAQ',
      cell: (faq) => (
        <div className="space-y-1">
          <div className="font-medium text-mono">{faq.name}</div>
          <div className="text-sm text-muted-foreground">{faq.language}</div>
        </div>
      ),
    },
    {
      key: 'status',
      header: 'Status',
      cell: (faq) => <FaqStatusBadge status={faq.status} />,
    },
    {
      key: 'sort',
      header: 'Sort strategy',
      cell: (faq) => <SortStrategyBadge value={faq.sortStrategy} />,
    },
    {
      key: 'cta',
      header: 'CTA',
      cell: (faq) => (faq.ctaEnabled ? 'Enabled' : 'Disabled'),
    },
    {
      key: 'actions',
      header: 'Actions',
      className: 'w-[180px]',
      cell: (faq) => (
        <div
          className="flex items-center justify-end gap-1"
          onClick={(event) => event.stopPropagation()}
        >
          <Button asChild variant="ghost" mode="icon">
            <Link to={`/app/faq/${faq.id}/edit`}>
              <Pencil className="size-4" />
            </Link>
          </Button>
          <Button
            variant="ghost"
            mode="icon"
            onClick={() => {
              void requestGeneration.mutateAsync(faq.id);
            }}
          >
            <WandSparkles className="size-4" />
          </Button>
          <Button
            variant="ghost"
            mode="icon"
            onClick={() => {
              if (window.confirm(`Delete FAQ "${faq.name}"?`)) {
                void deleteFaq.mutateAsync(faq.id);
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
          eyebrow="FAQ"
          title="FAQs"
          description="Shape knowledge spaces, review readiness, and open each FAQ to manage answers and sources."
          actions={
            <Button asChild>
              <Link to="/app/faq/new">
                <Plus className="size-4" />
                New FAQ
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
            placeholder="Search FAQs"
          />
          <Select value={statusFilter} onValueChange={setStatusFilter}>
            <SelectTrigger>
              <SelectValue placeholder="Filter by status" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All statuses</SelectItem>
              {Object.entries(faqStatusLabels).map(([value, label]) => (
                <SelectItem key={value} value={value}>
                  {label}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
          <Select value={sorting} onValueChange={setSorting}>
            <SelectTrigger>
              <SelectValue placeholder="Sort FAQs" />
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
            title: 'Catalog size',
            value: faqQuery.data?.totalCount ?? 0,
            description: deferredSearch ? `Search: ${deferredSearch}` : activeStatusLabel,
          },
          {
            title: 'Published on page',
            value: publishedCount,
            description: publishedCount ? 'Ready for customer traffic' : 'Nothing published in this slice',
          },
          {
            title: 'Drafts on page',
            value: draftCount,
            description: draftCount ? 'Still being curated' : 'No draft work in view',
          },
          {
            title: 'CTA enabled',
            value: ctaEnabledCount,
            description: `${sortingLabel} order`,
          },
        ]}
      />
      <DataTable
        title="Knowledge spaces"
        description="Open a FAQ to review answers, source links, and generation readiness."
        columns={columns}
        rows={faqRows}
        getRowId={(row) => row.id}
        loading={faqQuery.isLoading}
        onRowClick={(faq) => navigate(`/app/faq/${faq.id}`)}
        toolbar={
          <>
            <Badge variant="outline">{faqQuery.data?.totalCount ?? 0} total</Badge>
            <Badge variant={statusFilter === 'all' ? 'outline' : 'info'} appearance="outline">
              {activeStatusLabel}
            </Badge>
          </>
        }
        emptyState={
          <EmptyState
            title="No FAQs in view"
            description="Create a FAQ to start shaping this workspace knowledge base."
            action={{ label: 'Create FAQ', to: '/app/faq/new' }}
          />
        }
        errorState={
          faqQuery.isError ? (
            <ErrorState
              title="Unable to load FAQs"
              description="Refresh the catalog and try again."
              retry={() => void faqQuery.refetch()}
            />
          ) : undefined
        }
        footer={
          faqQuery.data ? (
            <PaginationControls
              page={page}
              pageSize={pageSize}
              totalCount={faqQuery.data.totalCount}
              onPageChange={setPage}
            />
          ) : undefined
        }
      />
    </ListLayout>
  );
}
