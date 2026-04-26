import { useEffect } from 'react';
import {
  CheckCircle2,
  Pencil,
  Plus,
  ShieldCheck,
  Trash2,
  Waypoints,
} from 'lucide-react';
import { Link, useNavigate } from 'react-router-dom';
import { usePortalTimeZone } from '@/domains/settings/settings-hooks';
import { useDeleteSource, useSourceList } from '@/domains/sources/hooks';
import type { SourceDto } from '@/domains/sources/types';
import {
  VisibilityScope,
  sourceKindLabels,
  visibilityScopeLabels,
} from '@/shared/constants/backend-enums';
import { ListLayout, PageHeader, SectionGrid } from '@/shared/layout/page-layouts';
import { clampPage } from '@/shared/lib/pagination';
import { formatNumericDateTimeInTimeZone } from '@/shared/lib/time-zone';
import { useListQueryState } from '@/shared/lib/use-list-query-state';
import { translateText } from '@/shared/lib/i18n-core';
import { DataTable, type DataTableColumn } from '@/shared/ui/data-table';
import { PaginationControls } from '@/shared/ui/pagination-controls';
import { EmptyState, ErrorState } from '@/shared/ui/placeholder-state';
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
} from '@/shared/ui';
import { SourceKindBadge, VisibilityBadge } from '@/shared/ui/status-badges';

const sortingOptions = [
  { value: 'Label ASC', label: 'Label A-Z' },
  { value: 'Kind ASC', label: 'Source kind' },
  { value: 'LastVerifiedAtUtc DESC', label: 'Recently verified' },
  { value: 'Locator ASC', label: 'Locator' },
];

const SOURCE_FILTER_DEFAULTS = {
  kind: 'all',
  visibility: 'all',
  authoritative: 'all',
} as const;

export function SourceListPage() {
  const navigate = useNavigate();
  const portalTimeZone = usePortalTimeZone();
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
    defaultSorting: 'Label ASC',
    filterDefaults: SOURCE_FILTER_DEFAULTS,
  });
  const kindFilter = filters.kind;
  const visibilityFilter = filters.visibility;
  const authoritativeFilter = filters.authoritative;
  const apiKind = kindFilter === 'all' ? undefined : Number(kindFilter);
  const apiVisibility =
    visibilityFilter === 'all' ? undefined : Number(visibilityFilter);
  const apiAuthoritative =
    authoritativeFilter === 'all'
      ? undefined
      : authoritativeFilter === 'true';

  const sourceQuery = useSourceList({
    page,
    pageSize,
    sorting,
    searchText: debouncedSearch || undefined,
    kind: apiKind,
    visibility: apiVisibility,
    isAuthoritative: apiAuthoritative,
  });

  useEffect(() => {
    const totalCount = sourceQuery.data?.totalCount;

    if (totalCount === undefined) {
      return;
    }

    const nextPage = clampPage(page, totalCount, pageSize);
    if (nextPage !== page) {
      setPage(nextPage, { replace: true });
    }
  }, [page, pageSize, setPage, sourceQuery.data?.totalCount]);

  const deleteSource = useDeleteSource();
  const sourceRows = sourceQuery.data?.items ?? [];
  const authoritativeCount = sourceRows.filter((source) => source.isAuthoritative).length;
  const publicCitationCount = sourceRows.filter(
    (source) =>
      source.allowsPublicCitation && source.visibility >= VisibilityScope.Public,
  ).length;
  const verifiedCount = sourceRows.filter((source) => Boolean(source.lastVerifiedAtUtc)).length;
  const showMetricsLoadingState =
    sourceQuery.isLoading && sourceQuery.data === undefined;

  const columns: DataTableColumn<SourceDto>[] = [
    {
      key: 'source',
      header: 'Source',
      cell: (source) => (
        <div className="space-y-1">
          <div className="font-medium text-mono">
            {source.label || translateText('Untitled source')}
          </div>
          <div className="break-all text-sm text-muted-foreground">{source.locator}</div>
          <div className="text-sm text-muted-foreground">
            {source.language}
            {source.contextNote ? ` • ${source.contextNote}` : ''}
          </div>
        </div>
      ),
    },
    {
      key: 'kind',
      header: 'Type',
      className: 'lg:w-[180px]',
      cell: (source) => (
        <div className="space-y-2">
          <SourceKindBadge kind={source.kind} />
          <VisibilityBadge visibility={source.visibility} />
        </div>
      ),
    },
    {
      key: 'authoritative',
      header: 'Trust',
      className: 'lg:w-[180px]',
      cell: (source) => (
        <div className="space-y-2 text-sm text-muted-foreground">
          <Badge variant={source.isAuthoritative ? 'primary' : 'outline'}>
            {translateText(source.isAuthoritative ? 'Authoritative' : 'Reference')}
          </Badge>
          {source.allowsPublicCitation ? (
            <Badge variant="success" appearance="outline">
              {translateText('Public citation')}
            </Badge>
          ) : null}
          {source.allowsPublicExcerpt ? (
            <Badge variant="outline">{translateText('Public excerpt')}</Badge>
          ) : null}
        </div>
      ),
    },
    {
      key: 'lastVerifiedAtUtc',
      header: 'Verified',
      className: 'lg:w-[160px]',
      cell: (source) => (
        <span className="text-sm text-muted-foreground">
          {formatNumericDateTimeInTimeZone(source.lastVerifiedAtUtc, portalTimeZone)}
        </span>
      ),
    },
    {
      key: 'actions',
      header: 'Actions',
      className: 'lg:w-[120px]',
      cell: (source) => (
        <div
          className="flex items-center justify-end gap-1"
          onClick={(event) => event.stopPropagation()}
        >
          <Button asChild variant="ghost" mode="icon">
            <Link to={`/app/sources/${source.id}/edit`}>
              <Pencil className="size-4" />
            </Link>
          </Button>
          <ConfirmAction
            title={translateText('Delete source "{name}"?', {
              name: source.label || source.locator,
            })}
            description={translateText(
              'This removes the source from the portal catalog and from future attachment flows.',
            )}
            confirmLabel={translateText('Delete source')}
            isPending={deleteSource.isPending}
            onConfirm={() => deleteSource.mutateAsync(source.id)}
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
          title="Sources"
          description="Maintain the evidence, citations, and reusable reference material behind questions and answers."
          descriptionMode="inline"
          actions={
            <Button asChild>
              <Link to="/app/sources/new">
                <Plus className="size-4" />
                {translateText('New source')}
              </Link>
            </Button>
          }
        />
      }
    >
      {showMetricsLoadingState ? (
        <SectionGridSkeleton />
      ) : (
        <SectionGrid
          items={[
            {
              title: 'Total',
              value: sourceQuery.data?.totalCount ?? 0,
              description: debouncedSearch
                ? translateText('Search: {value}', { value: debouncedSearch })
                : translateText('Reusable source records in this workspace'),
              icon: Waypoints,
            },
            {
              title: 'Authoritative',
              value: authoritativeCount,
              description: translateText('Strongest source-of-truth records'),
              icon: ShieldCheck,
            },
            {
              title: 'Public citation',
              value: publicCitationCount,
              description: translateText('Sources that can be exposed publicly'),
              icon: CheckCircle2,
            },
            {
              title: 'Verified',
              value: verifiedCount,
              description: translateText('Sources with a verification timestamp'),
              icon: ShieldCheck,
            },
          ]}
        />
      )}
      <DataTable
        title="Sources"
        description="Open a source to review trust metadata and external identifiers."
        descriptionMode="hint"
        columns={columns}
        rows={sourceRows}
        getRowId={(row) => row.id}
        loading={sourceQuery.isLoading}
        onRowClick={(source) => navigate(`/app/sources/${source.id}`)}
        toolbar={
          <div className="grid w-full gap-2 sm:grid-cols-2 xl:grid-cols-[minmax(240px,1fr)_220px_220px_220px]">
            <div className="sm:col-span-2 xl:col-span-1">
              <Input
                value={search}
                onChange={(event) => setSearch(event.target.value)}
                placeholder={translateText('Search sources')}
              />
            </div>
            <Select value={kindFilter} onValueChange={(value) => setFilter('kind', value)}>
              <SelectTrigger className="w-full">
                <SelectValue placeholder={translateText('Source kind')} />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All kinds</SelectItem>
                {Object.entries(sourceKindLabels).map(([value, label]) => (
                  <SelectItem key={value} value={value}>
                    {translateText(label)}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <Select
              value={visibilityFilter}
              onValueChange={(value) => setFilter('visibility', value)}
            >
              <SelectTrigger className="w-full">
                <SelectValue placeholder={translateText('Visibility')} />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All visibility</SelectItem>
                {Object.entries(visibilityScopeLabels).map(([value, label]) => (
                  <SelectItem key={value} value={value}>
                    {translateText(label)}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <Select
              value={authoritativeFilter}
              onValueChange={(value) => setFilter('authoritative', value)}
            >
              <SelectTrigger className="w-full">
                <SelectValue placeholder={translateText('Authoritative state')} />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All trust states</SelectItem>
                <SelectItem value="true">Authoritative only</SelectItem>
                <SelectItem value="false">Reference only</SelectItem>
              </SelectContent>
            </Select>
            <div className="sm:col-span-2 xl:col-span-4">
              <Select value={sorting} onValueChange={setSorting}>
                <SelectTrigger className="w-full xl:max-w-[240px]">
                  <SelectValue placeholder={translateText('Sort sources')} />
                </SelectTrigger>
                <SelectContent>
                  {sortingOptions.map((option) => (
                    <SelectItem key={option.value} value={option.value}>
                      {translateText(option.label)}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          </div>
        }
        emptyState={
          <EmptyState
            title="No sources in view"
            description="Create a source record so questions and answers can cite stable evidence."
            action={{ label: 'New source', to: '/app/sources/new' }}
          />
        }
        errorState={
          sourceQuery.isError ? (
            <ErrorState
              title="Unable to load sources"
              error={sourceQuery.error}
              retry={() => void sourceQuery.refetch()}
            />
          ) : undefined
        }
        footer={
          sourceQuery.data ? (
            <PaginationControls
              page={page}
              pageSize={pageSize}
              totalCount={sourceQuery.data.totalCount}
              onPageChange={setPage}
              onPageSizeChange={setPageSize}
              isFetching={sourceQuery.isFetching}
            />
          ) : undefined
        }
      />
    </ListLayout>
  );
}
