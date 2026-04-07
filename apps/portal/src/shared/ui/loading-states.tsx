import { ReactNode } from "react";
import { Skeleton } from "@/components/ui/skeleton";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardHeading,
  CardTitle,
  CardToolbar,
} from "@/components/ui/card";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";

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

export function SectionGridSkeleton({
  items = 4,
}: {
  items?: number;
}) {
  return (
    <div className="grid gap-5 sm:grid-cols-2 xl:grid-cols-4 lg:gap-7.5">
      {Array.from({ length: items }, (_, index) => (
        <Card key={`section-grid-skeleton-${index}`} className="bg-muted/10">
          <CardContent className="relative min-w-0 p-5">
            <div className="min-w-0 space-y-2.5">
              <Skeleton className="h-3 w-24" />
              <Skeleton className="h-8 w-28" />
              <Skeleton className="h-4 w-40" />
            </div>
            <Skeleton className="absolute right-5 top-5 size-11 rounded-2xl" />
          </CardContent>
        </Card>
      ))}
    </div>
  );
}

export function KeyValueListSkeleton({
  items = 4,
}: {
  items?: number;
}) {
  return (
    <div className="overflow-hidden rounded-xl border border-border/70">
      {Array.from({ length: items }, (_, index) => (
        <div
          key={`key-value-list-skeleton-${index}`}
          className="flex flex-col gap-1.5 border-b border-border/70 px-4 py-3 last:border-b-0 sm:flex-row sm:items-start sm:justify-between sm:gap-4"
        >
          <Skeleton className="h-4 w-24" />
          <Skeleton className="h-4 w-full sm:w-48" />
        </div>
      ))}
    </div>
  );
}

export function TableCardSkeleton({
  title,
  description,
  toolbar,
  columns = 4,
  rows = 6,
}: {
  title?: ReactNode;
  description?: ReactNode;
  toolbar?: ReactNode;
  columns?: number;
  rows?: number;
}) {
  return (
    <Card>
      {title || description || toolbar ? (
        <CardHeader className="gap-4 md:flex-row md:items-start md:justify-between">
          <CardHeading>
            {title ? <CardTitle>{title}</CardTitle> : null}
            {description ? <CardDescription>{description}</CardDescription> : null}
          </CardHeading>
          {toolbar ? <CardToolbar className="flex-wrap gap-2">{toolbar}</CardToolbar> : null}
        </CardHeader>
      ) : null}

      <CardContent className="space-y-5">
        <div className="space-y-3 lg:hidden">
          {Array.from({ length: 4 }, (_, index) => (
            <div
              key={`mobile-table-card-skeleton-${index}`}
              className="rounded-xl border border-border/80 bg-card p-4"
            >
              <div className="space-y-3">
                {Array.from({ length: columns }, (_, columnIndex) => (
                  <div
                    key={`mobile-table-row-skeleton-${index}-${columnIndex}`}
                    className="space-y-1.5"
                  >
                    <Skeleton className="h-3 w-20" />
                    <Skeleton className="h-5 w-[75%]" />
                  </div>
                ))}
              </div>
            </div>
          ))}
        </div>

        <div className="hidden overflow-hidden rounded-xl border border-border/80 bg-card lg:block">
          <Table>
            <TableHeader className="bg-muted/40">
              <TableRow>
                {Array.from({ length: columns }, (_, index) => (
                  <TableHead key={`table-head-skeleton-${index}`}>
                    <Skeleton className="h-4 w-20" />
                  </TableHead>
                ))}
              </TableRow>
            </TableHeader>
            <TableBody>
              {Array.from({ length: rows }, (_, rowIndex) => (
                <TableRow key={`table-row-skeleton-${rowIndex}`}>
                  {Array.from({ length: columns }, (_, columnIndex) => (
                    <TableCell key={`table-cell-skeleton-${rowIndex}-${columnIndex}`}>
                      <Skeleton className="h-5 w-[75%]" />
                    </TableCell>
                  ))}
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
      </CardContent>
    </Card>
  );
}

export function TileGridSkeleton({
  items = 4,
  className = "grid gap-4 md:grid-cols-2",
}: {
  items?: number;
  className?: string;
}) {
  return (
    <div className={className}>
      {Array.from({ length: items }, (_, index) => (
        <div
          key={`tile-grid-skeleton-${index}`}
          className="rounded-2xl border border-border bg-muted/15 p-4"
        >
          <div className="flex items-start justify-between gap-3">
            <div className="space-y-2">
              <Skeleton className="h-5 w-28" />
              <Skeleton className="h-4 w-36" />
            </div>
            <Skeleton className="h-6 w-24 rounded-full" />
          </div>
          <div className="mt-3 space-y-2">
            <Skeleton className="h-4 w-full" />
            <Skeleton className="h-4 w-[70%]" />
          </div>
        </div>
      ))}
    </div>
  );
}
