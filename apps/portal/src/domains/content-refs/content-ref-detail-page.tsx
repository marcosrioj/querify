import { Pencil, Plus, Trash2 } from "lucide-react";
import { useMemo } from "react";
import {
  Link,
  useNavigate,
  useParams,
  useSearchParams,
} from "react-router-dom";
import { useFaqList } from "@/domains/faq/hooks";
import { useFaqItemList } from "@/domains/faq-items/hooks";
import {
  useContentRef,
  useDeleteContentRef,
} from "@/domains/content-refs/hooks";
import {
  DetailLayout,
  KeyValueList,
  PageHeader,
  SectionGrid,
} from "@/shared/layout/page-layouts";
import { useLocalPagination } from "@/shared/lib/use-local-pagination";
import {
  Badge,
  Button,
  Card,
  CardContent,
  CardHeader,
  CardHeading,
  CardTitle,
  ContextHint,
} from "@/shared/ui";
import { PaginationControls } from "@/shared/ui/pagination-controls";
import { ContentRefKindBadge } from "@/shared/ui/status-badges";
import { EmptyState, ErrorState } from "@/shared/ui/placeholder-state";

const DETAIL_PAGE_SIZE_OPTIONS = [5, 10, 20];

export function ContentRefDetailPage() {
  const navigate = useNavigate();
  const { id: faqId, contentRefId } = useParams();
  const [searchParams] = useSearchParams();
  const resolvedContentRefId = contentRefId;
  const originatingFaqItemId = searchParams.get("faqItemId") ?? "";
  const contentRefQuery = useContentRef(resolvedContentRefId);
  const faqItemQuery = useFaqItemList({
    page: 1,
    pageSize: 100,
    sorting: "Question ASC",
    faqId,
    contentRefId: resolvedContentRefId,
  });
  const deleteContentRef = useDeleteContentRef();

  const relatedItems = faqItemQuery.data?.items ?? [];
  const relatedFaqIds = useMemo(
    () =>
      Array.from(
        new Set([
          ...(faqId ? [faqId] : []),
          ...relatedItems.map((item) => item.faqId),
        ]),
      ),
    [faqId, relatedItems],
  );
  const faqQuery = useFaqList({
    page: 1,
    pageSize: Math.max(relatedFaqIds.length, 1),
    sorting: "Name ASC",
    faqIds: relatedFaqIds.length ? relatedFaqIds : undefined,
  });
  const relatedFaqs = useMemo(() => {
    const usageByFaq = new Map<string, number>();

    relatedItems.forEach((item) => {
      usageByFaq.set(item.faqId, (usageByFaq.get(item.faqId) ?? 0) + 1);
    });

    return (faqQuery.data?.items ?? [])
      .filter((faq) => usageByFaq.has(faq.id))
      .map((faq) => ({
        ...faq,
        usageCount: usageByFaq.get(faq.id) ?? 0,
      }));
  }, [faqQuery.data?.items, relatedItems]);
  const relatedFaqsPagination = useLocalPagination({
    items: relatedFaqs,
    defaultPageSize: DETAIL_PAGE_SIZE_OPTIONS[0],
  });
  const relatedItemsPagination = useLocalPagination({
    items: relatedItems,
    defaultPageSize: DETAIL_PAGE_SIZE_OPTIONS[0],
  });
  const resolvedFaqId = faqId ?? relatedFaqs[0]?.id ?? relatedItems[0]?.faqId;
  const backTo = resolvedFaqId ? `/app/faq/${resolvedFaqId}` : "/app/faq";
  const createFaqItemPath =
    resolvedFaqId && resolvedContentRefId
      ? `/app/faq/${resolvedFaqId}/items/new?contentRefId=${resolvedContentRefId}`
      : "/app/faq";
  const editPath =
    resolvedFaqId && resolvedContentRefId
      ? `/app/faq/${resolvedFaqId}/content-refs/${resolvedContentRefId}/edit`
      : backTo;
  const attachOriginItemPath =
    resolvedFaqId && originatingFaqItemId && resolvedContentRefId
      ? `/app/faq/${resolvedFaqId}/items/${originatingFaqItemId}/edit?contentRefId=${resolvedContentRefId}`
      : "";

  if (!resolvedContentRefId) {
    return (
      <ErrorState
        title="Invalid source route"
        description="Source routes need an identifier."
      />
    );
  }

  return (
    <DetailLayout
      header={
        <PageHeader
          eyebrow="Sources"
          title={contentRefQuery.data?.label || "Source"}
          description="See where this source is used and keep the reference up to date."
          descriptionMode="hint"
          backTo={backTo}
          actions={
            <>
              <Button asChild>
                <Link to={createFaqItemPath}>
                  <Plus className="size-4" />
                  New Q&A item
                </Link>
              </Button>
              {attachOriginItemPath ? (
                <Button asChild variant="outline">
                  <Link to={attachOriginItemPath}>Link to Q&A item</Link>
                </Button>
              ) : null}
              <Button asChild variant="outline">
                <Link to={editPath}>
                  <Pencil className="size-4" />
                  Edit
                </Link>
              </Button>
              <Button
                variant="destructive"
                onClick={() => {
                  if (
                    contentRefQuery.data &&
                    window.confirm(
                      `Delete source "${contentRefQuery.data.label || contentRefQuery.data.locator}"?`,
                    )
                  ) {
                    void deleteContentRef
                      .mutateAsync(resolvedContentRefId)
                      .then(() => navigate(backTo));
                  }
                }}
              >
                <Trash2 className="size-4" />
                Delete
              </Button>
            </>
          }
        />
      }
      sidebar={
        <Card>
          <CardHeader>
            <CardHeading>
              <CardTitle className="flex flex-wrap items-center gap-2">
                <span>Overview</span>
                <ContextHint
                  content="Source type, scope, and downstream usage."
                  label="Overview details"
                />
              </CardTitle>
            </CardHeading>
          </CardHeader>
          <CardContent>
            {contentRefQuery.data ? (
              <KeyValueList
                items={[
                  {
                    label: "Kind",
                    value: (
                      <ContentRefKindBadge kind={contentRefQuery.data.kind} />
                    ),
                  },
                  {
                    label: "Scope",
                    value: contentRefQuery.data.scope || "No scope",
                  },
                  { label: "FAQs", value: String(relatedFaqs.length) },
                  { label: "Q&A items", value: String(relatedItems.length) },
                ]}
              />
            ) : null}
          </CardContent>
        </Card>
      }
    >
      {contentRefQuery.isError ? (
        <ErrorState
          title="Unable to load source"
          description="The source request failed."
          retry={() => void contentRefQuery.refetch()}
        />
      ) : contentRefQuery.data ? (
        <>
          <SectionGrid
            items={[
              {
                title: "Type",
                value: <ContentRefKindBadge kind={contentRefQuery.data.kind} />,
                titleHint: "How this source is classified.",
              },
              {
                title: "Q&A items",
                value: relatedItems.length,
                description: relatedItems.length
                  ? "Q&A items already linked"
                  : "No Q&A items linked yet",
              },
              {
                title: "FAQs",
                value: relatedFaqs.length,
                description: relatedFaqs.length
                  ? "Knowledge spaces currently relying on it"
                  : "No FAQs currently depend on this source",
              },
              {
                title: "Scope",
                value: contentRefQuery.data.scope || "Not set",
                titleHint: "Optional grouping label.",
              },
            ]}
          />

          <Card>
            <CardHeader>
              <CardHeading>
                <CardTitle className="flex flex-wrap items-center gap-2">
                  <span>Reference</span>
                  <ContextHint
                    content="The link or path saved for this source."
                    label="Reference details"
                  />
                </CardTitle>
              </CardHeading>
            </CardHeader>
            <CardContent className="space-y-3">
              <p className="break-all text-sm leading-6">
                {contentRefQuery.data.locator}
              </p>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardHeading>
                <CardTitle className="flex flex-wrap items-center gap-2">
                  <span>FAQs</span>
                  <ContextHint
                    content="Knowledge spaces already drawing from this source."
                    label="FAQ usage details"
                  />
                </CardTitle>
              </CardHeading>
            </CardHeader>
            <CardContent className="space-y-3">
              {relatedFaqs.length ? (
                <>
                  {relatedFaqsPagination.pagedItems.map((faq) => (
                    <div
                      key={faq.id}
                      className="rounded-2xl border border-border bg-muted/15 p-4"
                    >
                      <div className="flex flex-col gap-3 md:flex-row md:items-start md:justify-between">
                        <div>
                          <p className="font-medium text-mono">{faq.name}</p>
                          <p className="mt-1 text-sm text-muted-foreground">
                            Used by {faq.usageCount} linked{" "}
                            {faq.usageCount === 1
                              ? "Q&A item"
                              : "Q&A items"}
                          </p>
                        </div>
                        <Button asChild variant="outline" size="sm">
                          <Link to={`/app/faq/${faq.id}`}>Open FAQ</Link>
                        </Button>
                      </div>
                    </div>
                  ))}
                  {relatedFaqsPagination.totalCount >
                  DETAIL_PAGE_SIZE_OPTIONS[0] ? (
                    <PaginationControls
                      page={relatedFaqsPagination.page}
                      pageSize={relatedFaqsPagination.pageSize}
                      totalCount={relatedFaqsPagination.totalCount}
                      onPageChange={relatedFaqsPagination.setPage}
                      onPageSizeChange={relatedFaqsPagination.setPageSize}
                      pageSizeOptions={DETAIL_PAGE_SIZE_OPTIONS}
                    />
                  ) : null}
                </>
              ) : (
                <EmptyState
                  title="No FAQs yet"
                  description="Link this source to a Q&A item so it shows up in a FAQ."
                  action={{ label: "New Q&A item", to: createFaqItemPath }}
                />
              )}
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardHeading>
                <CardTitle className="flex flex-wrap items-center gap-2">
                  <span>Q&A items</span>
                  <ContextHint
                    content="Q&A items currently linked to this source."
                    label="Q&A item usage details"
                  />
                </CardTitle>
              </CardHeading>
            </CardHeader>
            <CardContent className="space-y-3">
              {relatedItems.length ? (
                <>
                  {relatedItemsPagination.pagedItems.map((item) => (
                    <div
                      key={item.id}
                      className="rounded-2xl border border-border bg-muted/15 p-4"
                    >
                      <div className="flex items-start justify-between gap-3">
                        <div>
                          <div className="flex flex-wrap items-center gap-2">
                            <p className="font-medium text-mono">
                              {item.question}
                            </p>
                            <Badge variant={item.isActive ? "success" : "mono"}>
                              {item.isActive ? "Active" : "Inactive"}
                            </Badge>
                          </div>
                          <p className="mt-1 text-sm text-muted-foreground">
                            {item.shortAnswer}
                          </p>
                          <p className="mt-3 text-xs text-muted-foreground">
                            FAQ:{" "}
                            <Link
                              className="font-medium text-primary hover:underline"
                              to={`/app/faq/${item.faqId}`}
                            >
                              {relatedFaqs.find((faq) => faq.id === item.faqId)
                                ?.name ?? item.faqId}
                            </Link>
                          </p>
                        </div>
                        <Button asChild variant="outline" size="sm">
                          <Link to={`/app/faq/${item.faqId}/items/${item.id}`}>
                            Open
                          </Link>
                        </Button>
                      </div>
                    </div>
                  ))}
                  {relatedItemsPagination.totalCount >
                  DETAIL_PAGE_SIZE_OPTIONS[0] ? (
                    <PaginationControls
                      page={relatedItemsPagination.page}
                      pageSize={relatedItemsPagination.pageSize}
                      totalCount={relatedItemsPagination.totalCount}
                      onPageChange={relatedItemsPagination.setPage}
                      onPageSizeChange={relatedItemsPagination.setPageSize}
                      pageSizeOptions={DETAIL_PAGE_SIZE_OPTIONS}
                    />
                  ) : null}
                </>
              ) : (
                <EmptyState
                  title="No Q&A items yet"
                  description="Link this source to a Q&A item so teams can reuse it."
                  action={{ label: "New Q&A item", to: createFaqItemPath }}
                />
              )}
            </CardContent>
          </Card>
        </>
      ) : (
        <Card>
          <CardContent className="py-8 text-sm text-muted-foreground">
            Loading source…
          </CardContent>
        </Card>
      )}
    </DetailLayout>
  );
}
