import { Card, CardContent, CardHeader } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";

export function SidebarSummarySkeleton() {
  return (
    <Card>
      <CardHeader className="space-y-2">
        <Skeleton className="h-5 w-28" />
        <Skeleton className="h-4 w-40" />
      </CardHeader>
      <CardContent className="space-y-4">
        {Array.from({ length: 5 }, (_, index) => (
          <div
            key={`sidebar-skeleton-${index}`}
            className="flex items-center justify-between gap-4"
          >
            <Skeleton className="h-4 w-24" />
            <Skeleton className="h-4 w-20" />
          </div>
        ))}
      </CardContent>
    </Card>
  );
}

export function DetailPageSkeleton({
  cards = 3,
  metrics = 4,
}: {
  cards?: number;
  metrics?: number;
}) {
  return (
    <>
      <div className="grid gap-5 sm:grid-cols-2 xl:grid-cols-4 lg:gap-7.5">
        {Array.from({ length: metrics }, (_, index) => (
          <Card key={`detail-metric-skeleton-${index}`} className="bg-muted/10">
            <CardContent className="space-y-3 p-5">
              <Skeleton className="h-3 w-24" />
              <Skeleton className="h-8 w-20" />
              <Skeleton className="h-4 w-full" />
            </CardContent>
          </Card>
        ))}
      </div>

      {Array.from({ length: cards }, (_, index) => (
        <Card key={`detail-card-skeleton-${index}`}>
          <CardHeader className="space-y-2">
            <Skeleton className="h-5 w-36" />
            <Skeleton className="h-4 w-56" />
          </CardHeader>
          <CardContent className="space-y-4">
            {Array.from({ length: 4 }, (_, rowIndex) => (
              <div key={`detail-row-skeleton-${index}-${rowIndex}`} className="space-y-2">
                <Skeleton className="h-4 w-28" />
                <Skeleton className="h-4 w-full" />
              </div>
            ))}
          </CardContent>
        </Card>
      ))}
    </>
  );
}

export function FormCardSkeleton({
  fields = 6,
}: {
  fields?: number;
}) {
  return (
    <Card>
      <CardHeader className="space-y-2">
        <Skeleton className="h-5 w-28" />
        <Skeleton className="h-4 w-60" />
      </CardHeader>
      <CardContent className="space-y-4">
        {Array.from({ length: fields }, (_, index) => (
          <div key={`form-skeleton-${index}`} className="space-y-2">
            <Skeleton className="h-4 w-24" />
            <Skeleton className="h-10 w-full" />
          </div>
        ))}
        <div className="flex flex-wrap gap-3 pt-2">
          <Skeleton className="h-9 w-28" />
          <Skeleton className="h-9 w-24" />
        </div>
      </CardContent>
    </Card>
  );
}
