import { useEffect } from "react";
import {
  ArrowUpRight,
  BookOpen,
  FileText,
  Pencil,
  Plus,
  Sparkles,
  Trash2,
  WandSparkles,
} from "lucide-react";
import { Link, useNavigate } from "react-router-dom";
import {
  useDeleteFaq,
  useFaqList,
  useRequestFaqGeneration,
} from "@/domains/faq/hooks";
import { FaqDto } from "@/domains/faq/types";
import { faqStatusLabels, FaqStatus } from "@/shared/constants/backend-enums";
import {
  ListLayout,
  PageHeader,
  SectionGrid,
} from "@/shared/layout/page-layouts";
import { clampPage } from "@/shared/lib/pagination";
import { useListQueryState } from "@/shared/lib/use-list-query-state";
import { DataTable, type DataTableColumn } from "@/shared/ui/data-table";
import { PaginationControls } from "@/shared/ui/pagination-controls";
import { EmptyState, ErrorState } from "@/shared/ui/placeholder-state";
import { FaqStatusBadge } from "@/shared/ui/status-badges";
import {
  Badge,
  Button,
  ConfirmAction,
  Input,
  SectionGridSkeleton,
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/ui";

const sortingOptions = [
  { value: "UpdatedDate DESC", label: "Last updated" },
  { value: "Name ASC", label: "Name A-Z" },
  { value: "Language ASC", label: "Language" },
  { value: "Status ASC", label: "Status" },
];

const FAQ_FILTER_DEFAULTS = {
  status: "all",
} as const;

export function FaqListPage() {
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
    defaultSorting: "UpdatedDate DESC",
    filterDefaults: FAQ_FILTER_DEFAULTS,
  });
  const statusFilter = filters.status;
  const apiStatus = statusFilter === "all" ? undefined : Number(statusFilter);

  const faqQuery = useFaqList({
    page,
    pageSize,
    sorting,
    searchText: debouncedSearch || undefined,
    status: apiStatus,
  });

  useEffect(() => {
    const totalCount = faqQuery.data?.totalCount;

    if (totalCount === undefined) {
      return;
    }

    const nextPage = clampPage(page, totalCount, pageSize);
    if (nextPage !== page) {
      setPage(nextPage, { replace: true });
    }
  }, [faqQuery.data?.totalCount, page, pageSize, setPage]);

  const deleteFaq = useDeleteFaq();
  const requestGeneration = useRequestFaqGeneration();
  const faqRows = faqQuery.data?.items ?? [];
  const publishedCount = faqRows.filter(
    (faq) => faq.status === FaqStatus.Published,
  ).length;
  const draftCount = faqRows.filter(
    (faq) => faq.status === FaqStatus.Draft,
  ).length;
  const sortingLabel =
    sortingOptions.find((option) => option.value === sorting)?.label ??
    "Custom";
  const activeStatusLabel =
    statusFilter === "all"
      ? "All statuses"
      : faqStatusLabels[Number(statusFilter) as FaqStatus];
  const showMetricsLoadingState =
    faqQuery.isLoading && faqQuery.data === undefined;

  const columns: DataTableColumn<FaqDto>[] = [
    {
      key: "name",
      header: "FAQ",
      cell: (faq) => (
        <div className="space-y-1">
          <div className="font-medium text-mono">{faq.name}</div>
          <div className="text-sm text-muted-foreground">{faq.language}</div>
        </div>
      ),
    },
    {
      key: "status",
      header: "Status",
      cell: (faq) => <FaqStatusBadge status={faq.status} />,
    },
    {
      key: "actions",
      header: "Actions",
      className: "w-[180px]",
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
          <ConfirmAction
            title={`Run AI generation for "${faq.name}"?`}
            description="This queues generation for the FAQ and uses the configured AI provider setup for the current workspace."
            confirmLabel="Run generation"
            variant="primary"
            isPending={requestGeneration.isPending}
            onConfirm={() => requestGeneration.mutateAsync(faq.id)}
            trigger={
              <Button variant="ghost" mode="icon">
                <WandSparkles className="size-4" />
              </Button>
            }
          />
          <ConfirmAction
            title={`Delete FAQ "${faq.name}"?`}
            description="This removes the FAQ from the workspace. You will need to recreate it if you change your mind later."
            confirmLabel="Delete FAQ"
            isPending={deleteFaq.isPending}
            onConfirm={() => deleteFaq.mutateAsync(faq.id)}
            trigger={
              <Button variant="ghost" mode="icon">
                <Trash2 className="size-4 text-destructive" />
              </Button>
            }
          />
        </div>
      ),
    },
  ];

  return (
    <ListLayout
      header={
        <PageHeader
          eyebrow="FAQs"
          title="FAQs"
          description="Create clean FAQ spaces, then fill them with Q&A items and source material."
          descriptionMode="hint"
          actions={
            <Button asChild>
              <Link to="/app/faq/new">
                <Plus className="size-4" />
                {faqQuery.data?.totalCount ? "New FAQ" : "Start here"}
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
              placeholder="Search FAQs"
            />
          </div>
          <Select
            value={statusFilter}
            onValueChange={(value) => setFilter("status", value)}
          >
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
      {showMetricsLoadingState ? (
        <SectionGridSkeleton />
      ) : (
        <SectionGrid
          items={[
            {
              title: "Total",
              value: faqQuery.data?.totalCount ?? 0,
              description: debouncedSearch
                ? `Search: ${debouncedSearch}`
                : activeStatusLabel,
              icon: BookOpen,
            },
            {
              title: "Published",
              value: publishedCount,
              description: publishedCount
                ? "Ready for customer traffic"
                : "Nothing published in this slice",
              icon: Sparkles,
            },
            {
              title: "Drafts",
              value: draftCount,
              description: draftCount
                ? "Still being curated"
                : "No draft work in view",
              icon: FileText,
            },
            {
              title: "Order",
              value: sortingLabel,
              description: "Current sort applied",
              icon: ArrowUpRight,
            },
          ]}
        />
      )}
      <DataTable
        title="FAQs"
        description="Open a FAQ to review Q&A items, sources, and publish status."
        descriptionMode="hint"
        columns={columns}
        rows={faqRows}
        getRowId={(row) => row.id}
        loading={faqQuery.isLoading}
        onRowClick={(faq) => navigate(`/app/faq/${faq.id}`)}
        toolbar={
          <>
            <Badge variant="outline">
              {faqQuery.data?.totalCount ?? 0} total
            </Badge>
            <Badge
              variant={statusFilter === "all" ? "outline" : "info"}
              appearance="outline"
            >
              {activeStatusLabel}
            </Badge>
          </>
        }
        emptyState={
          <EmptyState
            title="No FAQs in view"
            description="Create a FAQ to start building help content for this workspace."
            action={{ label: "Start here", to: "/app/faq/new" }}
          />
        }
        errorState={
          faqQuery.isError ? (
            <ErrorState
              title="Unable to load FAQs"
              error={faqQuery.error}
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
              onPageSizeChange={setPageSize}
              isFetching={faqQuery.isFetching}
            />
          ) : undefined
        }
      />
    </ListLayout>
  );
}
