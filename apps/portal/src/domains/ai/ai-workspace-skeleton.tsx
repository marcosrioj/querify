import { PageHeader, PageSurface } from '@/shared/layout/page-layouts';
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardHeading,
  CardTitle,
  Skeleton,
} from '@/shared/ui';
import { SectionGridSkeleton, TileGridSkeleton } from '@/shared/ui/loading-states';

export function AiWorkspaceSkeleton() {
  return (
    <PageSurface className="space-y-5 lg:space-y-7.5">
      <PageHeader
        title="AI"
        description="Check providers and run FAQ generation."
      />

      <SectionGridSkeleton />

      <Card>
        <CardHeader>
          <CardHeading>
            <CardTitle>Configured providers</CardTitle>
            <CardDescription>
              Review what is ready for matching and generation.
            </CardDescription>
          </CardHeading>
        </CardHeader>
        <CardContent>
          <TileGridSkeleton items={3} className="grid gap-4 md:grid-cols-2 xl:grid-cols-3" />
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardHeading>
            <CardTitle>Generation launchpad</CardTitle>
            <CardDescription>
              Trigger generation directly from the latest FAQs.
            </CardDescription>
          </CardHeading>
        </CardHeader>
        <CardContent className="space-y-3">
          {Array.from({ length: 4 }, (_, index) => (
            <div
              key={`ai-launchpad-skeleton-${index}`}
              className="flex flex-col gap-3 rounded-2xl border border-border p-4 md:flex-row md:items-center md:justify-between"
            >
              <div className="space-y-2">
                <Skeleton className="h-5 w-44" />
                <Skeleton className="h-4 w-20" />
              </div>
              <div className="flex items-center gap-2">
                <Skeleton className="h-10 w-36" />
                <Skeleton className="h-10 w-24" />
              </div>
            </div>
          ))}
        </CardContent>
      </Card>
    </PageSurface>
  );
}
