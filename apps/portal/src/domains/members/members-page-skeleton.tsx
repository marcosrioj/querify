import { ListLayout, PageHeader } from '@/shared/layout/page-layouts';
import { Skeleton } from '@/shared/ui';
import { SectionGridSkeleton, TableCardSkeleton } from '@/shared/ui/loading-states';

export function MembersPageSkeleton() {
  return (
    <ListLayout
      header={
        <PageHeader
          title="Members"
          description="Manage workspace access for the current tenant."
          actions={<Skeleton className="h-10 w-32" />}
        />
      }
    >
      <SectionGridSkeleton />
      <TableCardSkeleton
        title="Members"
        description="See who has access to the current workspace and which role each person has."
        toolbar={
          <>
            <Skeleton className="h-6 w-20 rounded-full" />
            <Skeleton className="h-6 w-28 rounded-full" />
          </>
        }
        columns={3}
      />
    </ListLayout>
  );
}
