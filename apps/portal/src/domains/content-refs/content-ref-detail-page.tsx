import { ArrowUpRight, BookOpen, ExternalLink, FileText, FolderOpen, Link2, MessageSquare, Pencil, Plus, Trash2 } from "lucide-react";
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
  ConfirmAction,
  ContextHint,
  DetailPageSkeleton,
  ProgressChecklistCard,
  SidebarSummarySkeleton,
} from "@/shared/ui";
import { PaginationControls } from "@/shared/ui/pagination-controls";
import { ContentRefKindBadge } from "@/shared/ui/status-badges";
import { EmptyState, ErrorState } from "@/shared/ui/placeholder-state";
import { contentRefKindLabels } from "@/shared/constants/backend-enums";
import { translateText } from "@/shared/lib/i18n-core";

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
  const isExternalLocator = Boolean(
    contentRefQuery.data?.locator &&
      /^https?:\/\//i.test(contentRefQuery.data.locator),
  );
  const sourceSteps = [
    {
      id: "saved",
      label: "Save the reference",
      description: contentRefQuery.data
        ? "The underlying locator is already stored."
        : "Create the source record so teams can attach it to answers.",
      complete: Boolean(contentRefQuery.data),
    },
    {
      id: "label",
      label: "Label it for reuse",
      description: contentRefQuery.data?.label
        ? translateText("Current label: {value}", {
            value: contentRefQuery.data.label,
          })
        : "A clear label makes this source easier to find later.",
      complete: Boolean(contentRefQuery.data?.label),
    },
    {
      id: "items",
      label: "Link a Q&A item",
      description: relatedItems.length
        ? relatedItems.length === 1
          ? translateText("1 Q&A item already uses this source.")
          : translateText("{count} Q&A items already use this source.", {
              count: relatedItems.length,
            })
        : "Attach the source to at least one answer so it starts doing real work.",
      complete: relatedItems.length > 0,
    },
    {
      id: "faqs",
      label: "Reuse it in a FAQ",
      description: relatedFaqs.length
        ? relatedFaqs.length === 1
          ? translateText("1 FAQ already depends on it.")
          : translateText("{count} FAQs already depend on it.", {
              count: relatedFaqs.length,
            })
        : "Once a linked answer exists, this source becomes part of FAQ coverage.",
      complete: relatedFaqs.length > 0,
    },
  ];
  const nextSourceAction =
    !contentRefQuery.data?.label
      ? { label: "Add a label", to: editPath }
      : relatedItems.length === 0
        ? { label: "Link a Q&A item", to: createFaqItemPath }
        : { label: "Edit source", to: editPath };
  const showLoadingState =
    !contentRefQuery.data &&
    (contentRefQuery.isLoading || faqItemQuery.isLoading || faqQuery.isLoading);

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
          title={contentRefQuery.data?.label || "Source"}
          description="See where this source is used and keep the reference up to date."
          descriptionMode="hint"
          backTo={backTo}
        />
      }
      sidebar={
        <>
          <Card>
            <CardContent className="grid grid-cols-2 gap-2 p-3">
              <Button asChild size="sm" className="w-full justify-start">
                <Link to={createFaqItemPath}>
                  <Plus className="size-4" />
                  {translateText("New Q&A item")}
                </Link>
              </Button>
              <Button asChild variant="outline" size="sm" className="w-full justify-start">
                <Link to={editPath}>
                  <Pencil className="size-4" />
                  {translateText("Edit")}
                </Link>
              </Button>
              {attachOriginItemPath ? (
                <Button asChild variant="outline" size="sm" className="w-full justify-start">
                  <Link to={attachOriginItemPath}>
                    <Link2 className="size-4" />
                    {translateText("Link to Q&A item")}
                  </Link>
                </Button>
              ) : null}
              <div className="col-span-2 h-px bg-border/50" />
              <ConfirmAction
                title={translateText('Delete source "{name}"?', {
                  name:
                    contentRefQuery.data?.label ||
                    contentRefQuery.data?.locator ||
                    translateText('this source'),
                })}
                description="This removes the source record from the portal. Keep it only if you no longer want it reused."
                confirmLabel="Delete source"
                isPending={deleteContentRef.isPending}
                onConfirm={() =>
                  deleteContentRef
                    .mutateAsync(resolvedContentRefId)
                    .then(() => navigate(backTo))
                }
                trigger={
                  <Button variant="destructive" size="sm" className="col-span-2 w-full justify-start">
                    <Trash2 className="size-4" />
                    {translateText("Delete")}
                  </Button>
                }
              />
            </CardContent>
          </Card>

          {showLoadingState ? (
            <SidebarSummarySkeleton />
          ) : contentRefQuery.data ? (
            <Card>
              <CardHeader>
                <CardHeading>
                  <CardTitle className="flex flex-wrap items-center gap-2">
                    <span>{translateText("Overview")}</span>
                    <ContextHint
                      content="Source type, scope, and downstream usage."
                      label="Overview details"
                    />
                  </CardTitle>
                </CardHeading>
              </CardHeader>
              <CardContent>
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
              </CardContent>
            </Card>
          ) : null}
        </>
      }
    >
      {contentRefQuery.isError ? (
        <ErrorState
          title="Unable to load source"
          error={contentRefQuery.error}
          retry={() => void contentRefQuery.refetch()}
        />
      ) : showLoadingState ? (
        <DetailPageSkeleton cards={2} />
      ) : contentRefQuery.data ? (
        <>
          <SectionGrid
            items={[
              {
                title: "Type",
                value: contentRefKindLabels[contentRefQuery.data.kind],
                titleHint: "How this source is classified.",
                icon: FileText,
              },
              {
                title: "Q&A items",
                value: relatedItems.length,
                description: relatedItems.length
                  ? "Q&A items already linked"
                  : "No Q&A items linked yet",
                icon: MessageSquare,
              },
              {
                title: "FAQs",
                value: relatedFaqs.length,
                description: relatedFaqs.length
                  ? "Knowledge spaces currently relying on it"
                  : "No FAQs currently depend on this source",
                icon: BookOpen,
              },
              {
                title: "Scope",
                value: contentRefQuery.data.scope || "Not set",
                titleHint: "Optional grouping label.",
                icon: FolderOpen,
              },
            ]}
          />

          <Card>
            <CardHeader>
              <CardHeading>
                <CardTitle className="flex flex-wrap items-center gap-2">
                  <span>{translateText("Reference")}</span>
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
              {isExternalLocator ? (
                <Button asChild variant="outline" size="sm">
                  <a
                    href={contentRefQuery.data.locator}
                    target="_blank"
                    rel="noreferrer"
                  >
                    <ExternalLink className="size-4" />
                    {translateText("Open reference")}
                  </a>
                </Button>
              ) : null}
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardHeading>
                <CardTitle className="flex flex-wrap items-center gap-2">
                  <span>{translateText("FAQs")}</span>
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
                            {translateText(
                              faq.usageCount === 1
                                ? "Used by {count} linked Q&A item"
                                : "Used by {count} linked Q&A items",
                              { count: faq.usageCount },
                            )}
                          </p>
                        </div>
                        <Button asChild variant="outline" size="sm">
                          <Link to={`/app/faq/${faq.id}`}>
                            <ArrowUpRight className="size-4" />
                            {translateText("Open FAQ")}
                          </Link>
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
                  <span>{translateText("Q&A items")}</span>
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
                              {translateText(item.isActive ? "Active" : "Inactive")}
                            </Badge>
                          </div>
                          <p className="mt-1 text-sm text-muted-foreground">
                            {item.shortAnswer || translateText("No answers yet")}
                          </p>
                          <p className="mt-3 text-xs text-muted-foreground">
                            {translateText("FAQ")}:{" "}
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
                            <ArrowUpRight className="size-4" />
                            {translateText("Open")}
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

          <ProgressChecklistCard
            title="Turn this source into something reusable"
            description="A good source is labeled, easy to find, and linked to answers teams already trust."
            steps={sourceSteps}
            action={nextSourceAction}
            secondaryAction={
              resolvedFaqId ? { label: "Open FAQ", to: backTo } : undefined
            }
          />
        </>
      ) : (
        <DetailPageSkeleton cards={2} />
      )}
    </DetailLayout>
  );
}
