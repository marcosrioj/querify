import { Pencil, Plus, Trash2, WandSparkles } from 'lucide-react';
import { Link, useNavigate } from 'react-router-dom';
import { useMemo, useState } from 'react';
import { useDeleteFaq, useFaqList, useRequestFaqGeneration } from '@/domains/faq/hooks';
import { faqStatusLabels, FaqStatus, faqSortStrategyLabels } from '@/shared/constants/backend-enums';
import { FaqDto } from '@/domains/faq/types';
import { ListLayout, PageHeader } from '@/shared/layout/page-layouts';
import { DataTable, type DataTableColumn } from '@/shared/ui/data-table';
import { PaginationControls } from '@/shared/ui/pagination-controls';
import { EmptyState, ErrorState } from '@/shared/ui/placeholder-state';
import { FaqStatusBadge, SortStrategyBadge } from '@/shared/ui/status-badges';
import { Button, Input, Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/shared/ui';

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

  const faqQuery = useFaqList({ page, pageSize, sorting });
  const deleteFaq = useDeleteFaq();
  const requestGeneration = useRequestFaqGeneration();

  const rows = useMemo(() => {
    return (faqQuery.data?.items ?? []).filter((faq) => {
      const matchesSearch =
        faq.name.toLowerCase().includes(search.toLowerCase()) ||
        faq.language.toLowerCase().includes(search.toLowerCase());
      const matchesStatus =
        statusFilter === 'all' || faq.status === Number(statusFilter);

      return matchesSearch && matchesStatus;
    });
  }, [faqQuery.data?.items, search, statusFilter]);

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
          description="Backed by the real FAQ Portal CRUD endpoints. Sorting and paging are server-driven; text search is client-side on the loaded page until the backend exposes filter contracts."
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
            placeholder="Search loaded FAQs"
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
      <DataTable
        title="Catalog"
        description="Current page filters are local because the API does not yet expose FAQ search parameters."
        columns={columns}
        rows={rows}
        getRowId={(row) => row.id}
        loading={faqQuery.isLoading}
        onRowClick={(faq) => navigate(`/app/faq/${faq.id}`)}
        emptyState={
          <EmptyState
            title="No FAQs on this page"
            description="Create a FAQ to start structuring your tenant knowledge base."
            action={{ label: 'Create FAQ', to: '/app/faq/new' }}
          />
        }
        errorState={
          faqQuery.isError ? (
            <ErrorState
              title="Unable to load FAQs"
              description="The FAQ Portal API request failed."
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
