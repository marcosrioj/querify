import { Link2, Pencil, Trash2 } from "lucide-react";
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
  ContextHint,
} from "@/shared/ui";
import { EmptyState, ErrorState } from "@/shared/ui/placeholder-state";
import { ContentRefKindBadge } from "@/shared/ui/status-badges";

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
  const contentRefPath =
    linkedContentRef && resolvedFaqId
      ? `/app/faq/${resolvedFaqId}/content-refs/${linkedContentRef.id}`
      : "";
  const createContentRefPath =
    resolvedFaqId && resolvedItemId
      ? `/app/faq/${resolvedFaqId}/content-refs/new?faqItemId=${resolvedItemId}`
      : backTo;
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
          eyebrow="Q&A items"
          title={itemQuery.data?.question ?? "Q&A item"}
          description="Review the question, answer, CTA, and source for this item."
          descriptionMode="hint"
          backTo={backTo}
          actions={
            <>
              {resolvedFaqId ? (
                <Button asChild>
                  <Link to={`/app/faq/${resolvedFaqId}`}>
                    <Link2 className="size-4" />
                    Open FAQ
                  </Link>
                </Button>
              ) : null}
              {contentRefPath ? (
                <Button asChild variant="outline">
                  <Link to={contentRefPath}>Source</Link>
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
                    itemQuery.data &&
                    window.confirm(
                      `Delete Q&A item "${itemQuery.data.question}"?`,
                    )
                  ) {
                    void deleteFaqItem
                      .mutateAsync(resolvedItemId)
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
                  content="Ranking, visibility, and relationship details."
                  label="Overview details"
                />
              </CardTitle>
            </CardHeading>
          </CardHeader>
          <CardContent>
            {itemQuery.data ? (
              <KeyValueList
                items={[
                  {
                    label: "Status",
                    value: (
                      <Badge
                        variant={itemQuery.data.isActive ? "success" : "mono"}
                      >
                        {itemQuery.data.isActive ? "Active" : "Inactive"}
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
                      "None linked",
                  },
                  { label: "Sort", value: String(itemQuery.data.sort) },
                  {
                    label: "Vote score",
                    value: String(itemQuery.data.voteScore),
                  },
                  {
                    label: "AI confidence",
                    value: String(itemQuery.data.aiConfidenceScore),
                  },
                ]}
              />
            ) : null}
          </CardContent>
        </Card>
      }
    >
      {itemQuery.isError ? (
        <ErrorState
          title="Unable to load Q&A item"
          description="The Q&A item request failed."
          retry={() => void itemQuery.refetch()}
        />
      ) : itemQuery.data ? (
        <>
          <SectionGrid
            items={[
              {
                title: "Answer depth",
                value: answerState,
                titleHint: "Current answer depth.",
              },
              {
                title: "CTA",
                value: itemQuery.data.ctaUrl ? "Configured" : "Missing",
                description:
                  itemQuery.data.ctaTitle || itemQuery.data.ctaUrl
                    ? "This answer can drive the next step"
                    : "No CTA configured for this answer",
              },
              {
                title: "Source",
                value: linkedContentRef ? "Linked" : "Missing",
                description: linkedContentRef
                  ? "Connected to reusable source material"
                  : "Attach a source to improve traceability",
              },
              {
                title: "FAQ",
                value: parentFaq?.name ?? "Unknown FAQ",
                titleHint: "FAQ this Q&A item belongs to.",
              },
            ]}
          />

          <Card>
            <CardHeader>
              <CardHeading>
                <CardTitle className="flex flex-wrap items-center gap-2">
                  <span>Question & answer</span>
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
                  Question
                </p>
                <p className="mt-2 text-sm leading-6">
                  {itemQuery.data.question}
                </p>
              </div>
              <div>
                <p className="text-xs font-medium uppercase tracking-[0.2em] text-muted-foreground">
                  Short answer
                </p>
                <p className="mt-2 text-sm leading-6">
                  {itemQuery.data.shortAnswer}
                </p>
              </div>
              {itemQuery.data.answer ? (
                <div>
                  <p className="text-xs font-medium uppercase tracking-[0.2em] text-muted-foreground">
                    Full answer
                  </p>
                  <p className="mt-2 whitespace-pre-wrap text-sm leading-6">
                    {itemQuery.data.answer}
                  </p>
                </div>
              ) : null}
              {itemQuery.data.additionalInfo ? (
                <div>
                  <p className="text-xs font-medium uppercase tracking-[0.2em] text-muted-foreground">
                    Additional info
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
                  <span>Links</span>
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
                  FAQ
                </p>
                <p className="mt-2 font-medium text-mono">
                  {parentFaq?.name ?? itemQuery.data.faqId}
                </p>
                <p className="mt-1 text-sm text-muted-foreground">
                  Publishing and visibility flow from this FAQ.
                </p>
                {parentFaq ? (
                  <Button asChild variant="outline" size="sm" className="mt-4">
                    <Link to={backTo}>Open FAQ</Link>
                  </Button>
                ) : null}
              </div>

              <div className="rounded-2xl border border-border bg-muted/15 p-4">
                <p className="text-xs font-medium uppercase tracking-[0.2em] text-muted-foreground">
                  Source
                </p>
                {linkedContentRef ? (
                  <>
                    <div className="mt-2 flex flex-wrap items-center gap-2">
                      <p className="font-medium text-mono">
                        {linkedContentRef.label || "Untitled source"}
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
                      <Link to={contentRefPath}>Open source</Link>
                    </Button>
                  </>
                ) : (
                  <>
                    <p className="mt-2 font-medium text-mono">
                      No source linked
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
                      <Link to={createContentRefPath}>New source</Link>
                    </Button>
                  </>
                )}
              </div>
            </CardContent>
          </Card>
        </>
      ) : (
        <Card>
          <CardContent className="py-8 text-sm text-muted-foreground">
            Loading Q&A item…
          </CardContent>
        </Card>
      )}
    </DetailLayout>
  );
}
