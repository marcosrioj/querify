import { ExternalLink, Pencil, Trash2 } from 'lucide-react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { usePortalTimeZone } from '@/domains/settings/settings-hooks';
import { useDeleteSource, useSource } from '@/domains/sources/hooks';
import { DetailLayout, KeyValueList, PageHeader, SectionGrid } from '@/shared/layout/page-layouts';
import {
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
} from '@/shared/ui';
import { EmptyState, ErrorState } from '@/shared/ui/placeholder-state';
import { SourceKindBadge, VisibilityBadge } from '@/shared/ui/status-badges';
import { translateText } from '@/shared/lib/i18n-core';
import { formatOptionalDateTimeInTimeZone } from '@/shared/lib/time-zone';

export function SourceDetailPage() {
  const navigate = useNavigate();
  const portalTimeZone = usePortalTimeZone();
  const { id } = useParams();
  const sourceQuery = useSource(id);
  const deleteSource = useDeleteSource();

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
          title={sourceQuery.data?.label || 'Source'}
          description="Review trust metadata, public-use rules, and connector identifiers for this reusable source."
          descriptionMode="hint"
          backTo="/app/sources"
        />
      }
      sidebar={
        <>
          <Card>
            <CardContent className="grid grid-cols-2 gap-2 p-3">
              <Button asChild size="sm" className="w-full justify-start">
                <Link to={`/app/sources/${id}/edit`}>
                  <Pencil className="size-4" />
                  {translateText('Edit')}
                </Link>
              </Button>
              <ConfirmAction
                title={translateText('Delete source "{name}"?', {
                  name: sourceQuery.data?.label || sourceQuery.data?.locator || translateText('this source'),
                })}
                description={translateText(
                  'This removes the source from the portal catalog and future linking flows.',
                )}
                confirmLabel={translateText('Delete source')}
                isPending={deleteSource.isPending}
                onConfirm={() => deleteSource.mutateAsync(id).then(() => navigate('/app/sources'))}
                trigger={
                  <Button
                    variant="destructive"
                    size="sm"
                    className="col-span-2 w-full justify-start"
                  >
                    <Trash2 className="size-4" />
                    {translateText('Delete')}
                  </Button>
                }
              />
            </CardContent>
          </Card>
          {sourceQuery.isLoading ? (
            <SidebarSummarySkeleton />
          ) : sourceQuery.data ? (
            <Card>
              <CardHeader>
                <CardHeading>
                  <CardTitle>{translateText('Overview')}</CardTitle>
                </CardHeading>
              </CardHeader>
              <CardContent>
                <KeyValueList
                  items={[
                    {
                      label: 'System name',
                      value: sourceQuery.data.systemName || 'Not set',
                    },
                    {
                      label: 'External ID',
                      value: sourceQuery.data.externalId || 'Not set',
                    },
                    {
                      label: 'Checksum',
                      value: sourceQuery.data.checksum || 'Not set',
                    },
                    {
                      label: 'Language',
                      value: sourceQuery.data.language || 'Not set',
                    },
                    {
                      label: 'Last verified',
                      value: formatOptionalDateTimeInTimeZone(
                        sourceQuery.data.lastVerifiedAtUtc,
                        portalTimeZone,
                        translateText('Not set'),
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
                title: 'Kind',
                value: <SourceKindBadge kind={sourceQuery.data.kind} />,
              },
              {
                title: 'Visibility',
                value: <VisibilityBadge visibility={sourceQuery.data.visibility} />,
              },
              {
                title: 'Citation',
                value: translateText(
                  sourceQuery.data.allowsPublicCitation ? 'Allowed' : 'Internal only',
                ),
              },
              {
                title: 'Excerpt',
                value: translateText(
                  sourceQuery.data.allowsPublicExcerpt ? 'Allowed' : 'Internal only',
                ),
              },
              {
                title: 'Trust',
                value: translateText(
                  sourceQuery.data.isAuthoritative ? 'Authoritative' : 'Reference',
                ),
              },
            ]}
          />
          <Card>
            <CardHeader>
              <CardHeading>
                <CardTitle className="flex items-center gap-2">
                  <span>{translateText('Locator')}</span>
                  <ContextHint
                    content={translateText(
                      'Use the canonical locator whenever possible so downstream links stay stable.',
                    )}
                    label={translateText('Locator details')}
                  />
                </CardTitle>
              </CardHeading>
            </CardHeader>
            <CardContent className="space-y-4">
              <p className="break-all text-sm leading-6">{sourceQuery.data.locator}</p>
              {/^https?:\/\//i.test(sourceQuery.data.locator) ? (
                <Button asChild variant="outline" size="sm">
                  <a href={sourceQuery.data.locator} target="_blank" rel="noreferrer">
                    <ExternalLink className="size-4" />
                    {translateText('Open locator')}
                  </a>
                </Button>
              ) : null}
            </CardContent>
          </Card>
          <Card>
            <CardHeader>
              <CardHeading>
                <CardTitle>{translateText('Metadata')}</CardTitle>
              </CardHeading>
            </CardHeader>
            <CardContent className="space-y-4">
              <KeyValueList
                items={[
                  { label: 'Scope', value: sourceQuery.data.scope || 'Not set' },
                  { label: 'Media type', value: sourceQuery.data.mediaType || 'Not set' },
                  {
                    label: 'Captured at',
                    value: formatOptionalDateTimeInTimeZone(
                      sourceQuery.data.capturedAtUtc,
                      portalTimeZone,
                      translateText('Not set'),
                    ),
                  },
                  {
                    label: 'Last verified',
                    value: formatOptionalDateTimeInTimeZone(
                      sourceQuery.data.lastVerifiedAtUtc,
                      portalTimeZone,
                      translateText('Not set'),
                    ),
                  },
                ]}
              />
              {sourceQuery.data.metadataJson ? (
                <pre className="overflow-x-auto rounded-2xl border border-border bg-muted/10 p-4 text-sm">
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
