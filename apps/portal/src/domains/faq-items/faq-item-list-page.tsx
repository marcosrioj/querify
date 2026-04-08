import { useEffect, useMemo } from "react";
import {
  CircleAlert,
  Link2,
  MessageSquare,
  Pencil,
  Plus,
  ShieldCheck,
  Trash2,
} from "lucide-react";
import { Link, useNavigate } from "react-router-dom";
import { useContentRefList } from "@/domains/content-refs/hooks";
import { useFaqList } from "@/domains/faq/hooks";
import { useDeleteFaqItem, useFaqItemList } from "@/domains/faq-items/hooks";
import type { FaqItemDto } from "@/domains/faq-items/types";
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
import { translateText } from "@/shared/lib/i18n-core";
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
  { value: "Question ASC", label: "Question A-Z" },
  { value: "VoteScore DESC", label: "Vote score" },
  { value: "AiConfidenceScore DESC", label: "AI confidence" },
];

const FAQ_ITEM_FILTER_DEFAULTS = {
  active: "all",
  faq: "all",
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
    defaultSorting: "UpdatedDate DESC",
    filterDefaults: FAQ_ITEM_FILTER_DEFAULTS,
  });
  const activeFilter = filters.active;
  const faqFilter = filters.faq;
  const apiFaqId = faqFilter === "all" ? undefined : faqFilter;
  const apiIsActive =
    activeFilter === "all" ? undefined : activeFilter === "true";

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

  const faqOptionsQuery = useFaqList({
    page: 1,
    pageSize: 100,
    sorting: "Name ASC",
  });
  const contentRefQuery = useContentRefList({
    page: 1,
    pageSize: 100,
    sorting: "Label ASC",
  });
  const deleteFaqItem = useDeleteFaqItem();
  const itemRows = faqItemQuery.data?.items ?? [];

  const faqLookup = useMemo(
    () =>
      Object.fromEntries(
        (faqOptionsQuery.data?.items ?? []).map((faq) => [faq.id, faq.name]),
      ),
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
    activeFilter === "all"
      ? "All states"
      : activeFilter === "true"
        ? "Active only"
        : "Inactive only";
  const selectedFaqLabel =
    faqFilter === "all" ? "All FAQs" : (faqLookup[faqFilter] ?? "Selected FAQ");
  const showMetricsLoadingState =
    faqItemQuery.isLoading && faqItemQuery.data === undefined;

  const columns: DataTableColumn<FaqItemDto>[] = [
    {
      key: "question",
      header: "Q&A item",
      cell: (item) => (
        <div className="space-y-1">
          <div className="font-medium text-mono">{item.question}</div>
          <div className="text-sm text-muted-foreground">
            {item.shortAnswer}
          </div>
        </div>
      ),
    },
    {
      key: "faq",
      header: "FAQ",
      cell: (item) => faqLookup[item.faqId] ?? item.faqId,
    },
    {
      key: "status",
      header: "Status",
      cell: (item) => (
        <Badge variant={item.isActive ? "success" : "mono"}>
          {item.isActive ? "Active" : "Inactive"}
        </Badge>
      ),
    },
    {
      key: "contentRef",
      header: "Source",
      cell: (item) =>
        item.contentRefId
          ? (contentRefLookup[item.contentRefId] ?? item.contentRefId)
          : "None",
    },
    {
      key: "actions",
      header: "Actions",
      className: "w-[140px]",
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
          <ConfirmAction
            title={`Delete Q&A item "${item.question}"?`}
            description="This removes the answer record from the workspace list. Keep it only if you no longer need it."
            confirmLabel="Delete Q&A item"
            isPending={deleteFaqItem.isPending}
            onConfirm={() => deleteFaqItem.mutateAsync(item.id)}
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
          title="Q&A items"
          description="Manage each question, answer, score, and source link."
          descriptionMode="hint"
          actions={
            <Button asChild>
              <Link to="/app/faq-items/new">
                <Plus className="size-4" />
                New Q&A item
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
              placeholder="Search Q&A items"
            />
          </div>
          <Select
            value={faqFilter}
            onValueChange={(value) => setFilter("faq", value)}
          >
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
          <Select
            value={activeFilter}
            onValueChange={(value) => setFilter("active", value)}
          >
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
              <SelectValue placeholder="Sort Q&A items" />
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
              value: faqItemQuery.data?.totalCount ?? 0,
              description: debouncedSearch
                ? translateText("Search: {value}", { value: debouncedSearch })
                : selectedFaqLabel,
              icon: MessageSquare,
            },
            {
              title: "Active",
              value: activeCount,
              description: activeFilterLabel,
              icon: ShieldCheck,
            },
            {
              title: "Sourced",
              value: sourcedCount,
              titleHint: sourcedCount
                ? "Already linked to a source."
                : "No sources linked in this slice.",
              icon: Link2,
            },
            {
              title: "Unsourced",
              value: unsourcedCount,
              titleHint: "Candidates for enrichment.",
              icon: CircleAlert,
            },
          ]}
        />
      )}
      <DataTable
        title="Q&A items"
        description="Open a Q&A item to review the question, answer, score, and CTA."
        descriptionMode="hint"
        columns={columns}
        rows={itemRows}
        getRowId={(row) => row.id}
        loading={faqItemQuery.isLoading}
        onRowClick={(item) => navigate(`/app/faq-items/${item.id}`)}
        toolbar={
          <>
            <Badge variant="outline">
              {faqItemQuery.data?.totalCount ?? 0} total
            </Badge>
            <Badge
              variant={activeFilter === "all" ? "outline" : "success"}
              appearance="outline"
            >
              {activeFilterLabel}
            </Badge>
            {faqFilter !== "all" ? (
              <Badge variant="info" appearance="outline">
                {selectedFaqLabel}
              </Badge>
            ) : null}
          </>
        }
        emptyState={
          <EmptyState
            title="No Q&A items in view"
            description="Create a Q&A item and link it to the right FAQ."
            action={{ label: "New Q&A item", to: "/app/faq-items/new" }}
          />
        }
        errorState={
          faqItemQuery.isError ? (
            <ErrorState
              title="Unable to load Q&A items"
              error={faqItemQuery.error}
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
