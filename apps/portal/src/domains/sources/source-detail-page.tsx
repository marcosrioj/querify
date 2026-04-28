import { useState } from "react";
import {
  CheckCircle2,
  ExternalLink,
  FolderKanban,
  MessageSquareText,
  Pencil,
  Trash2,
} from "lucide-react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { QnaModuleNav } from "@/domains/qna/qna-module-nav";
import { usePortalTimeZone } from "@/domains/settings/settings-hooks";
import { useDeleteSource, useSource } from "@/domains/sources/hooks";
import {
  DetailLayout,
  KeyValueList,
  PageHeader,
  SectionGrid,
} from "@/shared/layout/page-layouts";
import {
  ActionButton,
  ActionPanel,
  Button,
  Card,
  CardContent,
  CardHeader,
  CardHeading,
  CardTitle,
  ConfirmAction,
  ContextHint,
  DetailPageSkeleton,
  SidebarSummarySkeleton,
} from "@/shared/ui";
import { EmptyState, ErrorState } from "@/shared/ui/placeholder-state";
import { SourceKindBadge, VisibilityBadge } from "@/shared/ui/status-badges";
import { translateText } from "@/shared/lib/i18n-core";
import { formatOptionalDateTimeInTimeZone } from "@/shared/lib/time-zone";

export function SourceDetailPage() {
  const navigate = useNavigate();
  const portalTimeZone = usePortalTimeZone();
  const { id } = useParams();
  const sourceQuery = useSource(id);
  const deleteSource = useDeleteSource();
  const [relationshipTab, setRelationshipTab] = useState("spaces");

  if (!id) {
    return (
      <ErrorState
        title="Invalid source route"
        description="Source detail routes need an identifier."
      />
    );
  }

  return (
    <DetailLayout
      header={
        <PageHeader
          title={sourceQuery.data?.label || "Source"}
          description="Review trust metadata, public-use rules, and connector identifiers for this reusable source."
          descriptionMode="hint"
          backTo="/app/sources"
        />
      }
      sidebar={
        <>
          <ActionPanel description="Source actions and risk controls.">
            <ActionButton asChild tone="primary">
              <Link to={`/app/sources/${id}/edit`}>
                <Pencil className="size-4" />
                {translateText("Edit")}
              </Link>
            </ActionButton>
            <ConfirmAction
              title={translateText('Delete source "{name}"?', {
                name:
                  sourceQuery.data?.label ||
                  sourceQuery.data?.locator ||
                  translateText("this source"),
              })}
              description={translateText(
                "This removes the source from the portal catalog and future linking flows.",
              )}
              confirmLabel={translateText("Delete source")}
              isPending={deleteSource.isPending}
              onConfirm={() =>
                deleteSource
                  .mutateAsync(id)
                  .then(() => navigate("/app/sources"))
              }
              trigger={
                <ActionButton tone="danger">
                  <Trash2 className="size-4" />
                  {translateText("Delete")}
                </ActionButton>
              }
            />
          </ActionPanel>
          {sourceQuery.isLoading ? (
            <SidebarSummarySkeleton />
          ) : sourceQuery.data ? (
            <Card>
              <CardHeader>
                <CardHeading>
                  <CardTitle>{translateText("Overview")}</CardTitle>
                </CardHeading>
              </CardHeader>
              <CardContent>
                <KeyValueList
                  items={[
                    {
                      label: "External ID",
                      value: sourceQuery.data.externalId || "Not set",
                    },
                    {
                      label: "Checksum",
                      value: sourceQuery.data.checksum || "Not set",
                    },
                    {
                      label: "Language",
                      value: sourceQuery.data.language || "Not set",
                    },
                    {
                      label: "Last verified",
                      value: formatOptionalDateTimeInTimeZone(
                        sourceQuery.data.lastVerifiedAtUtc,
                        portalTimeZone,
                        translateText("Not set"),
                      ),
                    },
                  ]}
                />
              </CardContent>
            </Card>
          ) : null}
        </>
      }
    >
      {sourceQuery.isError ? (
        <ErrorState
          title="Unable to load source"
          error={sourceQuery.error}
          retry={() => void sourceQuery.refetch()}
        />
      ) : sourceQuery.isLoading ? (
        <DetailPageSkeleton cards={4} />
      ) : sourceQuery.data ? (
        <>
          <SectionGrid
            items={[
              {
                title: "Kind",
                value: <SourceKindBadge kind={sourceQuery.data.kind} />,
              },
              {
                title: "Visibility",
                value: (
                  <VisibilityBadge visibility={sourceQuery.data.visibility} />
                ),
              },
              {
                title: "Citation",
                value: translateText(
                  sourceQuery.data.allowsPublicCitation
                    ? "Allowed"
                    : "Internal only",
                ),
              },
              {
                title: "Excerpt",
                value: translateText(
                  sourceQuery.data.allowsPublicExcerpt
                    ? "Allowed"
                    : "Internal only",
                ),
              },
              {
                title: "Trust",
                value: translateText(
                  sourceQuery.data.isAuthoritative
                    ? "Authoritative"
                    : "Reference",
                ),
              },
            ]}
          />
          <Card>
            <CardHeader>
              <CardHeading>
                <CardTitle className="flex items-center gap-2">
                  <span>{translateText("Locator")}</span>
                  <ContextHint
                    content={translateText(
                      "Use the canonical locator whenever possible so downstream links stay stable.",
                    )}
                    label={translateText("Locator details")}
                  />
                </CardTitle>
              </CardHeading>
            </CardHeader>
            <CardContent className="space-y-4">
              <p className="break-all text-sm leading-6">
                {sourceQuery.data.locator}
              </p>
              {/^https?:\/\//i.test(sourceQuery.data.locator) ? (
                <Button asChild variant="outline" size="sm">
                  <a
                    href={sourceQuery.data.locator}
                    target="_blank"
                    rel="noreferrer"
                  >
                    <ExternalLink className="size-4" />
                    {translateText("Open locator")}
                  </a>
                </Button>
              ) : null}
            </CardContent>
          </Card>

          <QnaModuleNav
            eyebrow="Source relationships"
            activeKey={relationshipTab}
            onActiveKeyChange={setRelationshipTab}
            items={[
              {
                key: "spaces",
                label: "Spaces",
                description:
                  "Spaces curating this source as reusable evidence.",
                icon: FolderKanban,
                count: sourceQuery.data?.spaceUsageCount ?? 0,
              },
              {
                key: "questions",
                label: "Questions",
                description:
                  "Questions linked to this source for context or origin.",
                icon: MessageSquareText,
                count: sourceQuery.data?.questionUsageCount ?? 0,
              },
              {
                key: "answers",
                label: "Answers",
                description:
                  "Answers citing this source as supporting evidence.",
                icon: CheckCircle2,
                count: sourceQuery.data?.answerUsageCount ?? 0,
              },
            ]}
          />

          <Card>
            <CardHeader>
              <CardHeading>
                <CardTitle>
                  {translateText("Usage impact and metadata")}
                </CardTitle>
              </CardHeading>
            </CardHeader>
            <CardContent className="space-y-4">
              <KeyValueList
                items={[
                  {
                    label: "Context note",
                    value: sourceQuery.data.contextNote || "Not set",
                  },
                  {
                    label: "Media type",
                    value: sourceQuery.data.mediaType || "Not set",
                  },
                  {
                    label: "Captured at",
                    value: formatOptionalDateTimeInTimeZone(
                      sourceQuery.data.capturedAtUtc,
                      portalTimeZone,
                      translateText("Not set"),
                    ),
                  },
                  {
                    label: "Last verified",
                    value: formatOptionalDateTimeInTimeZone(
                      sourceQuery.data.lastVerifiedAtUtc,
                      portalTimeZone,
                      translateText("Not set"),
                    ),
                  },
                  {
                    label: "Spaces",
                    value: String(sourceQuery.data.spaceUsageCount),
                  },
                  {
                    label: "Questions",
                    value: String(sourceQuery.data.questionUsageCount),
                  },
                  {
                    label: "Answers",
                    value: String(sourceQuery.data.answerUsageCount),
                  },
                ]}
              />
              {sourceQuery.data.metadataJson ? (
                <pre className="overflow-x-auto rounded-lg border border-border bg-muted/10 p-4 text-sm">
                  {sourceQuery.data.metadataJson}
                </pre>
              ) : (
                <EmptyState
                  title="No metadata JSON"
                  description="Add structured metadata when an integration or ingestion flow needs it."
                />
              )}
            </CardContent>
          </Card>
        </>
      ) : null}
    </DetailLayout>
  );
}
