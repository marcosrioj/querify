import { ArrowUpRight, BookOpen, FileText, Files, Link2, Pencil, Plus, Trash2 } from "lucide-react";
import { useMemo } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { useFaqList } from "@/domains/faq/hooks";
import { useContentRef } from "@/domains/content-refs/hooks";
import { useDeleteFaqItem, useFaqItem } from "@/domains/faq-items/hooks";
import {
  DetailLayout,
  KeyValueList,
  PageHeader,
  SectionGrid,
} from "@/shared/layout/page-layouts";
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
import { EmptyState, ErrorState } from "@/shared/ui/placeholder-state";
import { ContentRefKindBadge } from "@/shared/ui/status-badges";
import { translateText } from "@/shared/lib/i18n-core";

export function FaqItemDetailPage() {
  const navigate = useNavigate();
  const { id: faqId, itemId } = useParams();
  const resolvedItemId = itemId;
  const itemQuery = useFaqItem(resolvedItemId);
  const faqOptionsQuery = useFaqList({
    page: 1,
    pageSize: 1,
    sorting: "Name ASC",
    faqIds: itemQuery.data?.faqId
      ? [itemQuery.data.faqId]
      : faqId
        ? [faqId]
        : undefined,
  });
  const linkedContentRefQuery = useContentRef(itemQuery.data?.contentRefId);
  const deleteFaqItem = useDeleteFaqItem();

  const parentFaq = faqOptionsQuery.data?.items.find(
    (faq) => faq.id === itemQuery.data?.faqId,
  );
  const resolvedFaqId = faqId ?? parentFaq?.id ?? itemQuery.data?.faqId;
  const linkedContentRef = linkedContentRefQuery.data;
  const backTo = resolvedFaqId ? `/app/faq/${resolvedFaqId}` : "/app/faq";
  const editPath =
    resolvedFaqId && resolvedItemId
      ? `/app/faq/${resolvedFaqId}/items/${resolvedItemId}/edit`
      : backTo;
  const faqSettingsPath = resolvedFaqId ? `/app/faq/${resolvedFaqId}/edit` : backTo;
  const contentRefPath =
    linkedContentRef && resolvedFaqId
      ? `/app/faq/${resolvedFaqId}/content-refs/${linkedContentRef.id}`
      : "";
  const createContentRefPath =
    resolvedFaqId && resolvedItemId
      ? `/app/faq/${resolvedFaqId}/content-refs/new?faqItemId=${resolvedItemId}`
      : backTo;
  const contentCoverageReady = Boolean(
    itemQuery.data?.answer || itemQuery.data?.additionalInfo,
  );
  const answerState = useMemo(() => {
    if (!itemQuery.data) {
      return "Loading";
    }

    if (itemQuery.data.answer && itemQuery.data.additionalInfo) {
      return "Full answer package";
    }

    if (itemQuery.data.answer) {
      return "Expanded answer";
    }

    return "Short answer only";
  }, [itemQuery.data]);
  const itemSteps = [
    {
      id: "question",
      label: "Capture the core answer",
      description: itemQuery.data
        ? "The question and short answer are already saved."
        : "Start by saving the core question and short answer.",
      complete: Boolean(itemQuery.data),
    },
    {
      id: "depth",
      label: "Add more context",
      description: contentCoverageReady
        ? "This item already goes beyond the short summary."
        : "Add a full answer or supporting notes so the item is more helpful than a one-line response.",
      complete: contentCoverageReady,
    },
    {
      id: "source",
      label: "Link a source",
      description: linkedContentRef
        ? "This answer is already tied to reusable source material."
        : "Attach a source to improve trust and traceability.",
      complete: Boolean(linkedContentRef),
    },
    {
      id: "visibility",
      label: "Make it active",
      description: itemQuery.data?.isActive
        ? "This answer is active and can surface in the FAQ."
        : "Inactive answers stay saved but hidden from end users.",
      complete: Boolean(itemQuery.data?.isActive),
    },
  ];
  const nextItemAction =
    !linkedContentRef && createContentRefPath
      ? { label: "Link a source", to: createContentRefPath }
      : !itemQuery.data?.isActive
        ? { label: "Review visibility", to: editPath }
        : { label: "Edit Q&A item", to: editPath };
  const showLoadingState =
    !itemQuery.data &&
    (itemQuery.isLoading ||
      faqOptionsQuery.isLoading ||
      linkedContentRefQuery.isLoading);

  if (!resolvedItemId) {
    return (
      <ErrorState
        title="Invalid Q&A item route"
        description="Q&A item routes need an identifier."
      />
    );
  }

  return (
    <DetailLayout
      header={
        <PageHeader
          title={itemQuery.data?.question ?? "Q&A item"}
          description="Review the question, answer, CTA, and source for this item."
          descriptionMode="hint"
          backTo={backTo}
        />
      }
      sidebar={
        <>
          <Card>
            <CardContent className="grid grid-cols-2 gap-2 p-3">
              {resolvedFaqId ? (
                <Button asChild size="sm" className="w-full justify-start">
                  <Link to={`/app/faq/${resolvedFaqId}`}>
                    <Link2 className="size-4" />
                    {translateText("Open FAQ")}
                  </Link>
                </Button>
              ) : null}
              <Button asChild variant="outline" size="sm" className="w-full justify-start">
                <Link to={editPath}>
                  <Pencil className="size-4" />
                  {translateText("Edit")}
                </Link>
              </Button>
              {contentRefPath ? (
                <Button asChild variant="outline" size="sm" className="w-full justify-start">
                  <Link to={contentRefPath}>
                    <Files className="size-4" />
                    {translateText("Open source")}
                  </Link>
                </Button>
              ) : null}
              <div className="col-span-2 h-px bg-border/50" />
              <ConfirmAction
                title={translateText('Delete Q&A item "{name}"?', {
                  name: itemQuery.data?.question ?? "this item",
                })}
                description="This removes the answer record from the FAQ workflow. Keep it only if you no longer need it."
                confirmLabel="Delete Q&A item"
                isPending={deleteFaqItem.isPending}
                onConfirm={() =>
                  deleteFaqItem
                    .mutateAsync(resolvedItemId)
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
          ) : itemQuery.data ? (
            <Card>
              <CardHeader>
                <CardHeading>
                  <CardTitle className="flex flex-wrap items-center gap-2">
                    <span>{translateText("Overview")}</span>
                    <ContextHint
                      content="Ranking, visibility, and relationship details."
                      label="Overview details"
                    />
                  </CardTitle>
                </CardHeading>
              </CardHeader>
              <CardContent>
                <KeyValueList
                  items={[
                    {
                      label: "Status",
                      value: (
                        <Badge
                          variant={itemQuery.data.isActive ? "success" : "mono"}
                        >
                          {translateText(itemQuery.data.isActive ? "Active" : "Inactive")}
                        </Badge>
                      ),
                    },
                    {
                      label: "FAQ",
                      value: parentFaq?.name ?? itemQuery.data.faqId,
                    },
                    {
                      label: "Source",
                      value:
                        linkedContentRef?.label ||
                        linkedContentRef?.locator ||
                        translateText("No source linked"),
                    },
                    { label: "Sort", value: String(itemQuery.data.sort) },
                    {
                      label: "Feedback score",
                      value: String(itemQuery.data.feedbackScore),
                    },
                    {
                      label: "AI confidence",
                      value: String(itemQuery.data.aiConfidenceScore),
                    },
                  ]}
                />
              </CardContent>
            </Card>
          ) : null}
        </>
      }
    >
      {itemQuery.isError ? (
        <ErrorState
          title="Unable to load Q&A item"
          error={itemQuery.error}
          retry={() => void itemQuery.refetch()}
        />
      ) : showLoadingState ? (
        <DetailPageSkeleton cards={2} />
      ) : itemQuery.data ? (
        <>
          <SectionGrid
            valueClassName="text-xl sm:text-xl"
            items={[
              {
                title: "Answer depth",
                value: answerState,
                titleHint: "Current answer depth.",
                icon: FileText,
              },
              {
                title: "CTA",
                value: itemQuery.data.ctaUrl ? "Configured" : "Missing",
                description: itemQuery.data.ctaTitle || itemQuery.data.ctaUrl
                  ? "This answer can drive the next step"
                  : "No CTA configured for this answer",
                icon: ArrowUpRight,
              },
              {
                title: "Source",
                value: linkedContentRef ? "Linked" : "Missing",
                description: linkedContentRef
                  ? "Connected to reusable source material"
                  : "Attach a source to improve traceability",
                icon: Files,
              },
              {
                title: "FAQ",
                value: parentFaq?.name ?? "Unknown FAQ",
                titleHint: "FAQ this Q&A item belongs to.",
                icon: BookOpen,
              },
            ]}
          />

          <Card>
              <CardHeader>
                <CardHeading>
                  <CardTitle className="flex flex-wrap items-center gap-2">
                    <span>{translateText("Question & answer")}</span>
                    <ContextHint
                      content="The question, answer, notes, and CTA live together here."
                      label="Question and answer details"
                  />
                </CardTitle>
              </CardHeading>
            </CardHeader>
            <CardContent className="space-y-4">
              <div>
                <p className="text-xs font-medium uppercase tracking-[0.2em] text-muted-foreground">
                  {translateText("Question")}
                </p>
                <p className="mt-2 text-sm leading-6">
                  {itemQuery.data.question}
                </p>
              </div>
              <div>
                <p className="text-xs font-medium uppercase tracking-[0.2em] text-muted-foreground">
                  {translateText("Short answer")}
                </p>
                <p className="mt-2 text-sm leading-6">
                  {itemQuery.data.shortAnswer}
                </p>
              </div>
              {itemQuery.data.answer ? (
                <div>
                  <p className="text-xs font-medium uppercase tracking-[0.2em] text-muted-foreground">
                    {translateText("Full answer")}
                  </p>
                  <p className="mt-2 whitespace-pre-wrap text-sm leading-6">
                    {itemQuery.data.answer}
                  </p>
                </div>
              ) : null}
              {itemQuery.data.additionalInfo ? (
                <div>
                  <p className="text-xs font-medium uppercase tracking-[0.2em] text-muted-foreground">
                    {translateText("Additional info")}
                  </p>
                  <p className="mt-2 whitespace-pre-wrap text-sm leading-6">
                    {itemQuery.data.additionalInfo}
                  </p>
                </div>
              ) : null}
              {itemQuery.data.ctaTitle || itemQuery.data.ctaUrl ? (
                <div className="rounded-2xl border border-border bg-muted/15 p-4">
                  <p className="font-medium text-mono">
                    {itemQuery.data.ctaTitle || "CTA"}
                  </p>
                  <p className="mt-1 text-sm text-muted-foreground">
                    {itemQuery.data.ctaUrl || "No URL configured"}
                  </p>
                </div>
              ) : (
                <EmptyState
                  title="No CTA"
                  description="Add a CTA if this Q&A item should drive an external action."
                />
              )}
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardHeading>
                <CardTitle className="flex flex-wrap items-center gap-2">
                  <span>{translateText("Links")}</span>
                  <ContextHint
                    content="See which FAQ and source this Q&A item belongs to."
                    label="Relationship details"
                  />
                </CardTitle>
              </CardHeading>
            </CardHeader>
            <CardContent className="grid gap-4 md:grid-cols-2">
              <div className="rounded-2xl border border-border bg-muted/15 p-4">
                <p className="text-xs font-medium uppercase tracking-[0.2em] text-muted-foreground">
                  {translateText("FAQ")}
                </p>
                <p className="mt-2 font-medium text-mono">
                  {parentFaq?.name ?? itemQuery.data.faqId}
                </p>
                <p className="mt-1 text-sm text-muted-foreground">
                  Publishing and visibility flow from this FAQ.
                </p>
                {parentFaq ? (
                  <Button asChild variant="outline" size="sm" className="mt-4">
                    <Link to={backTo}>
                      <ArrowUpRight className="size-4" />
                      {translateText("Open FAQ")}
                    </Link>
                  </Button>
                ) : null}
              </div>

              <div className="rounded-2xl border border-border bg-muted/15 p-4">
                <p className="text-xs font-medium uppercase tracking-[0.2em] text-muted-foreground">
                  {translateText("Source")}
                </p>
                {linkedContentRef ? (
                  <>
                    <div className="mt-2 flex flex-wrap items-center gap-2">
                      <p className="font-medium text-mono">
                        {linkedContentRef.label || translateText("Untitled source")}
                      </p>
                      <ContentRefKindBadge kind={linkedContentRef.kind} />
                    </div>
                    <p className="mt-1 break-all text-sm text-muted-foreground">
                      {linkedContentRef.locator}
                    </p>
                    <Button
                      asChild
                      variant="outline"
                      size="sm"
                      className="mt-4"
                    >
                      <Link to={contentRefPath}>
                        <ArrowUpRight className="size-4" />
                        {translateText("Open source")}
                      </Link>
                    </Button>
                  </>
                ) : (
                  <>
                    <p className="mt-2 font-medium text-mono">
                      {translateText("No source linked")}
                    </p>
                    <p className="mt-1 text-sm text-muted-foreground">
                      Link source material to improve quality and traceability.
                    </p>
                    <Button
                      asChild
                      variant="outline"
                      size="sm"
                      className="mt-4"
                    >
                      <Link to={createContentRefPath}>
                        <Plus className="size-4" />
                        {translateText("New source")}
                      </Link>
                    </Button>
                  </>
                )}
              </div>
            </CardContent>
          </Card>

          <ProgressChecklistCard
            title="Tighten this answer before it goes live"
            description="Use the checklist to spot missing depth, source coverage, or visibility settings without scanning the whole page."
            steps={itemSteps}
            action={nextItemAction}
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
