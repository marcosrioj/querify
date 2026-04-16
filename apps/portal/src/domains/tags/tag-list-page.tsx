import { useEffect } from 'react';
import { Pencil, Plus, Tags, Trash2 } from 'lucide-react';
import { Link, useNavigate } from 'react-router-dom';
import { useDeleteTag, useTagList } from '@/domains/tags/hooks';
import type { TagDto } from '@/domains/tags/types';
import { ListLayout, PageHeader, SectionGrid } from '@/shared/layout/page-layouts';
import { clampPage } from '@/shared/lib/pagination';
import { useListQueryState } from '@/shared/lib/use-list-query-state';
import { translateText } from '@/shared/lib/i18n-core';
import { DataTable, type DataTableColumn } from '@/shared/ui/data-table';
import { PaginationControls } from '@/shared/ui/pagination-controls';
import { EmptyState, ErrorState } from '@/shared/ui/placeholder-state';
import { Button, ConfirmAction, Input, SectionGridSkeleton } from '@/shared/ui';

const TAG_FILTER_DEFAULTS = {} as const;

export function TagListPage() {
  const navigate = useNavigate();
  const {
    debouncedSearch,
    page,
    pageSize,
    search,
    setPage,
    setPageSize,
    setSearch,
  } = useListQueryState({
    defaultSorting: 'Name ASC',
    filterDefaults: TAG_FILTER_DEFAULTS,
  });

  const tagQuery = useTagList({
    page,
    pageSize,
    sorting: 'Name ASC',
    searchText: debouncedSearch || undefined,
  });

  useEffect(() => {
    const totalCount = tagQuery.data?.totalCount;

    if (totalCount === undefined) {
      return;
    }

    const nextPage = clampPage(page, totalCount, pageSize);
    if (nextPage !== page) {
      setPage(nextPage, { replace: true });
    }
  }, [page, pageSize, setPage, tagQuery.data?.totalCount]);

  const deleteTag = useDeleteTag();
  const tagRows = tagQuery.data?.items ?? [];

  const columns: DataTableColumn<TagDto>[] = [
    {
      key: 'name',
      header: 'Tag',
      cell: (tag) => <div className="font-medium text-mono">{tag.name}</div>,
    },
    {
      key: 'actions',
      header: 'Actions',
      className: 'lg:w-[120px]',
      cell: (tag) => (
        <div
          className="flex items-center justify-end gap-1"
          onClick={(event) => event.stopPropagation()}
        >
          <Button asChild variant="ghost" mode="icon">
            <Link to={`/app/tags/${tag.id}/edit`}>
              <Pencil className="size-4" />
            </Link>
          </Button>
          <ConfirmAction
            title={translateText('Delete tag "{name}"?', { name: tag.name })}
            description={translateText(
              'This removes the reusable tag from the workspace taxonomy.',
            )}
            confirmLabel={translateText('Delete tag')}
            isPending={deleteTag.isPending}
            onConfirm={() => deleteTag.mutateAsync(tag.id)}
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
          title="Tags"
          description="Maintain the reusable taxonomy that groups spaces and questions."
          descriptionMode="inline"
          actions={
            <Button asChild>
              <Link to="/app/tags/new">
                <Plus className="size-4" />
                {translateText('New tag')}
              </Link>
            </Button>
          }
        />
      }
    >
      {tagQuery.isLoading && tagQuery.data === undefined ? (
        <SectionGridSkeleton />
      ) : (
        <SectionGrid
          items={[
            {
              title: 'Total',
              value: tagQuery.data?.totalCount ?? 0,
              description: debouncedSearch
                ? translateText('Search: {value}', { value: debouncedSearch })
                : translateText('Reusable taxonomy labels'),
              icon: Tags,
            },
          ]}
        />
      )}
      <DataTable
        title="Tags"
        description="Open a tag to rename it before attaching it elsewhere."
        descriptionMode="hint"
        columns={columns}
        rows={tagRows}
        getRowId={(row) => row.id}
        loading={tagQuery.isLoading}
        onRowClick={(tag) => navigate(`/app/tags/${tag.id}/edit`)}
        toolbar={
          <Input
            value={search}
            onChange={(event) => setSearch(event.target.value)}
            placeholder="Search tags"
            className="w-full max-w-sm"
          />
        }
        emptyState={
          <EmptyState
            title="No tags in view"
            description="Create the first reusable tag for space and question taxonomy."
            action={{ label: 'New tag', to: '/app/tags/new' }}
          />
        }
        errorState={
          tagQuery.isError ? (
            <ErrorState
              title="Unable to load tags"
              error={tagQuery.error}
              retry={() => void tagQuery.refetch()}
            />
          ) : undefined
        }
        footer={
          tagQuery.data ? (
            <PaginationControls
              page={page}
              pageSize={pageSize}
              totalCount={tagQuery.data.totalCount}
              onPageChange={setPage}
              onPageSizeChange={setPageSize}
              isFetching={tagQuery.isFetching}
            />
          ) : undefined
        }
      />
    </ListLayout>
  );
}
