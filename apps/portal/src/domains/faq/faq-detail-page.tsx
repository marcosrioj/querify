import {
  ArrowUpRight,
  Files,
  MessageSquare,
  Pencil,
  Plus,
  ShieldCheck,
  Trash2,
  WandSparkles,
} from "lucide-react";
import { useMemo } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import {
  useDeleteFaq,
  useFaq,
  useRequestFaqGeneration,
} from "@/domains/faq/hooks";
import { useFaqItemList } from "@/domains/faq-items/hooks";
import { useContentRefList } from "@/domains/content-refs/hooks";
import { FaqStatus } from "@/shared/constants/backend-enums";
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
import { ErrorState, EmptyState } from "@/shared/ui/placeholder-state";
import {
  ContentRefKindBadge,
  FaqStatusBadge,
} from "@/shared/ui/status-badges";
import { translateText } from "@/shared/lib/i18n-core";

const DETAIL_PAGE_SIZE_OPTIONS = [5, 10, 20];

export function FaqDetailPage() {
  const navigate = useNavigate();
  const { id } = useParams();
  const faqQuery = useFaq(id);
  const faqItemQuery = useFaqItemList({
    page: 1,
    pageSize: 100,
    sorting: "Question ASC",
    faqId: id,
  });
  const contentRefQuery = useContentRefList({
    page: 1,
    pageSize: 100,
    sorting: "Label ASC",
    faqId: id,
  });
  const deleteFaq = useDeleteFaq();
  const requestGeneration = useRequestFaqGeneration();

  const relatedItems = faqItemQuery.data?.items ?? [];
  const relatedContentRefs = useMemo(() => {
    const usageByContentRef = new Map<string, number>();

    relatedItems.forEach((item) => {
      if (!item.contentRefId) {
        return;
      }

      usageByContentRef.set(
        item.contentRefId,
        (usageByContentRef.get(item.contentRefId) ?? 0) + 1,
      );
    });

    return (contentRefQuery.data?.items ?? []).map((contentRef) => ({
      ...contentRef,
      usageCount: usageByContentRef.get(contentRef.id) ?? 0,
    }));
  }, [contentRefQuery.data?.items, relatedItems]);

  const activeItemCount = relatedItems.filter((item) => item.isActive).length;
  const generationReady =
    relatedItems.length > 0 && relatedContentRefs.length > 0;
  const relatedItemsPagination = useLocalPagination({
    items: relatedItems,
    defaultPageSize: DETAIL_PAGE_SIZE_OPTIONS[0],
  });
  const relatedContentRefsPagination = useLocalPagination({
    items: relatedContentRefs,
    defaultPageSize: DETAIL_PAGE_SIZE_OPTIONS[0],
  });
  const createFaqItemPath = `/app/faq/${id}/items/new`;
  const createContentRefPath = `/app/faq/${id}/content-refs/new`;
  const faqEditPath = `/app/faq/${id}/edit`;
  const setupSteps = [
    {
      id: "details",
      label: "Create the FAQ shell",
      description: faqQuery.data
        ? translateText("The FAQ exists as {language} content.", {
            language: faqQuery.data.language,
          })
        : "Create the FAQ record so the rest of the workflow has a home.",
      complete: Boolean(faqQuery.data),
    },
    {
      id: "items",
      label: "Add a Q&A item",
      description: relatedItems.length
        ? relatedItems.length === 1
          ? translateText("1 Q&A item already linked.")
          : translateText("{count} Q&A items already linked.", {
              count: relatedItems.length,
            })
        : "Add the first question and answer so this FAQ has usable content.",
      complete: relatedItems.length > 0,
    },
    {
      id: "sources",
      label: "Link a source",
      description: relatedContentRefs.length
        ? relatedContentRefs.length === 1
          ? translateText("1 source already connected.")
          : translateText("{count} sources already connected.", {
              count: relatedContentRefs.length,
            })
        : "Attach source material so answers are traceable and reusable.",
      complete: relatedContentRefs.length > 0,
    },
    {
      id: "publish",
      label: "Publish when ready",
      description:
        faqQuery.data?.status === FaqStatus.Published
          ? "This FAQ is already customer-facing."
          : "Keep it in draft until at least one answer and source are in place.",
      complete: faqQuery.data?.status === FaqStatus.Published,
    },
  ];
  const nextSetupAction =
    relatedItems.length === 0
      ? { label: "Start here", to: createFaqItemPath }
      : relatedContentRefs.length === 0
        ? { label: "Add a source", to: createContentRefPath }
        : faqQuery.data?.status !== FaqStatus.Published
          ? { label: "Review publish settings", to: faqEditPath }
          : { label: "Add another Q&A item", to: createFaqItemPath };
  const showLoadingState =
    !faqQuery.data &&
    (faqQuery.isLoading || faqItemQuery.isLoading || contentRefQuery.isLoading);

  if (!id) {
    return (
      <ErrorState
        title="Invalid FAQ route"
        description="FAQ detail routes require an identifier."
      />
    );
  }

  return (
    <DetailLayout
      header={
        <PageHeader
          title={faqQuery.data?.name ?? "FAQ"}
          description="See this FAQ, its Q&A items, sources, and publish status."
          descriptionMode="hint"
          backTo="/app/faq"
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
                <Link to={createContentRefPath}>
                  <Plus className="size-4" />
                  {translateText("New source")}
                </Link>
              </Button>
              <Button asChild variant="outline" size="sm" className="w-full justify-start">
                <Link to={faqEditPath}>
                  <Pencil className="size-4" />
                  {translateText("Edit")}
                </Link>
              </Button>
              <ConfirmAction
                title={translateText('Run AI generation for "{name}"?', {
                  name: faqQuery.data?.name ?? translateText("this FAQ"),
                })}
                description="This queues generation for the FAQ and uses the configured AI provider setup for the current workspace."
                confirmLabel="Run generation"
                variant="primary"
                isPending={requestGeneration.isPending}
                onConfirm={() => requestGeneration.mutateAsync(id)}
                trigger={
                  <Button variant="outline" size="sm" className="w-full justify-start">
                    <WandSparkles className="size-4" />
                    {translateText("AI generation")}
                  </Button>
                }
              />
              <div className="col-span-2 h-px bg-border/50" />
              <ConfirmAction
                title={translateText('Delete FAQ "{name}"?', {
                  name: faqQuery.data?.name ?? translateText("this FAQ"),
                })}
                description="This action removes the FAQ record from the portal. Review any linked content before continuing."
                confirmLabel="Delete FAQ"
                isPending={deleteFaq.isPending}
                onConfirm={() =>
                  deleteFaq.mutateAsync(id).then(() => navigate("/app/faq"))
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
          ) : faqQuery.data ? (
            <>
              <Card>
                <CardHeader>
                  <CardHeading>
                    <CardTitle className="flex flex-wrap items-center gap-2">
                      <span>{translateText("Overview")}</span>
                      <ContextHint
                        content="Key publishing and orchestration settings."
                        label="Overview details"
                      />
                    </CardTitle>
                  </CardHeading>
                </CardHeader>
                <CardContent>
                  <KeyValueList
                    items={[
                      { label: "Language", value: faqQuery.data.language },
                      {
                        label: "Q&A items",
                        value: String(relatedItems.length),
                      },
                      {
                        label: "Sources",
                        value: String(relatedContentRefs.length),
                      },
                    ]}
                  />
                </CardContent>
              </Card>

              <Card>
                <CardHeader>
                  <CardHeading>
                    <CardTitle className="flex flex-wrap items-center gap-2">
                      <span>{translateText("Status")}</span>
                      <ContextHint
                        content="Monitor readiness before you publish or request generation."
                        label="Status details"
                      />
                    </CardTitle>
                  </CardHeading>
                </CardHeader>
                <CardContent className="space-y-4">
                  <div className="flex flex-wrap gap-2">
                    <FaqStatusBadge status={faqQuery.data.status} />
                    <Badge variant={generationReady ? "success" : "warning"}>
                      {translateText(
                        generationReady
                          ? "Ready for generation request"
                          : "Needs content and Q&A coverage",
                      )}
                    </Badge>
                  </div>
                  <KeyValueList
                    items={[
                      {
                        label: "Visibility",
                        value:
                          faqQuery.data.status === FaqStatus.Published
                            ? "Customer-facing"
                            : "Internal or draft",
                      },
                      {
                        label: "Generation",
                        value: generationReady
                          ? "Ready to request"
                          : "Waiting on setup",
                      },
                      {
                        label: "Request tracking",
                        value: "Correlation id only",
                      },
                    ]}
                  />
                </CardContent>
              </Card>
            </>
          ) : null}
        </>
      }
    >
      {faqQuery.isError ? (
        <ErrorState
          title="Unable to load FAQ"
          error={faqQuery.error}
          retry={() => void faqQuery.refetch()}
        />
      ) : showLoadingState ? (
        <DetailPageSkeleton cards={3} />
      ) : faqQuery.data ? (
        <>
          <SectionGrid
            items={[
              {
                title: "Q&A items",
                value: relatedItems.length,
                description:
                  relatedItems.length === 1
                    ? translateText("1 item linked")
                    : translateText("{count} items linked", {
                        count: relatedItems.length,
                      }),
                icon: MessageSquare,
              },
              {
                title: "Active",
                value: activeItemCount,
                description:
                  activeItemCount === relatedItems.length
                    ? "Everything in view is active"
                    : "Some Q&A items still need activation",
                icon: ShieldCheck,
              },
              {
                title: "Sources",
                value: relatedContentRefs.length,
                description: relatedContentRefs.length
                  ? "Sources already connected"
                  : "No sources linked yet",
                icon: Files,
              },
              {
                title: "Ready",
                value: generationReady ? "Ready" : "Needs setup",
                description: generationReady
                  ? "Q&A items and sources are ready"
                  : "Add Q&A items and sources first",
                icon: WandSparkles,
              },
            ]}
          />

          <Card>
            <CardHeader>
              <CardHeading>
                <CardTitle className="flex flex-wrap items-center gap-2">
                  <span>{translateText("Q&A items")}</span>
                  <ContextHint
                    content="Q&A items currently attached to this FAQ."
                    label="Q&A item details"
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
                      <div className="flex flex-col gap-3 md:flex-row md:items-start md:justify-between">
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
                          <div className="mt-3 flex flex-wrap gap-2 text-xs text-muted-foreground">
                            <span>{translateText("Sort {value}", { value: item.sort })}</span>
                            <span>{translateText("Feedback {value}", { value: item.feedbackScore })}</span>
                            <span>{translateText("AI {value}", { value: item.aiConfidenceScore })}</span>
                            {item.contentRefId ? (
                              <span>
                                {translateText("Linked to")}{" "}
                                <Link
                                  className="font-medium text-primary hover:underline"
                                  to={`/app/faq/${id}/content-refs/${item.contentRefId}`}
                                >
                                  {translateText("Source")}
                                </Link>
                              </span>
                            ) : (
                              <span>{translateText("No source linked yet")}</span>
                            )}
                          </div>
                        </div>
                        <div className="flex flex-wrap gap-2">
                          <Button asChild variant="ghost" size="sm">
                            <Link to={`/app/faq/${id}/items/${item.id}/edit`}>
                              <Pencil className="size-4" />
                              {translateText("Edit")}
                            </Link>
                          </Button>
                          <Button asChild variant="outline" size="sm">
                            <Link to={`/app/faq/${id}/items/${item.id}`}>
                              <ArrowUpRight className="size-4" />
                              {translateText("Open")}
                            </Link>
                          </Button>
                        </div>
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
                  description="Create a Q&A item to start filling this FAQ."
                  action={{ label: "New Q&A item", to: createFaqItemPath }}
                />
              )}
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardHeading>
                <CardTitle className="flex flex-wrap items-center gap-2">
                  <span>{translateText("Sources")}</span>
                  <ContextHint
                    content="Source material already supporting this FAQ."
                    label="Source details"
                  />
                </CardTitle>
              </CardHeading>
            </CardHeader>
            <CardContent className="space-y-3">
              {relatedContentRefs.length ? (
                <>
                  {relatedContentRefsPagination.pagedItems.map((contentRef) => (
                    <div
                      key={contentRef.id}
                      className="rounded-2xl border border-border bg-muted/15 p-4"
                    >
                      <div className="flex flex-col gap-3 md:flex-row md:items-start md:justify-between">
                        <div>
                          <div className="flex flex-wrap items-center gap-2">
                            <p className="font-medium text-mono">
                              {contentRef.label || translateText("Untitled source")}
                            </p>
                            <ContentRefKindBadge kind={contentRef.kind} />
                          </div>
                          <p className="mt-1 break-all text-sm text-muted-foreground">
                            {contentRef.locator}
                          </p>
                          <p className="mt-3 text-xs text-muted-foreground">
                            {translateText(
                              contentRef.usageCount === 1
                                ? "Used by {count} linked Q&A item in this FAQ."
                                : "Used by {count} linked Q&A items in this FAQ.",
                              { count: contentRef.usageCount },
                            )}
                          </p>
                        </div>
                        <Button asChild variant="outline" size="sm">
                          <Link
                            to={`/app/faq/${id}/content-refs/${contentRef.id}`}
                          >
                            <ArrowUpRight className="size-4" />
                            {translateText("Open")}
                          </Link>
                        </Button>
                      </div>
                    </div>
                  ))}
                  {relatedContentRefsPagination.totalCount >
                  DETAIL_PAGE_SIZE_OPTIONS[0] ? (
                    <PaginationControls
                      page={relatedContentRefsPagination.page}
                      pageSize={relatedContentRefsPagination.pageSize}
                      totalCount={relatedContentRefsPagination.totalCount}
                      onPageChange={relatedContentRefsPagination.setPage}
                      onPageSizeChange={
                        relatedContentRefsPagination.setPageSize
                      }
                      pageSizeOptions={DETAIL_PAGE_SIZE_OPTIONS}
                    />
                  ) : null}
                </>
              ) : (
                <EmptyState
                  title="No sources yet"
                  description="Link sources to your Q&A items so generation has real content to use."
                  action={{ label: "New source", to: createContentRefPath }}
                />
              )}
            </CardContent>
          </Card>

          <ProgressChecklistCard
            title="Move this FAQ to a real result"
            description="Follow the next recommended step instead of hunting through every action in the screen."
            steps={setupSteps}
            action={nextSetupAction}
            secondaryAction={{ label: "Edit FAQ", to: faqEditPath }}
          />
        </>
      ) : (
        <DetailPageSkeleton cards={2} />
      )}
    </DetailLayout>
  );
}
