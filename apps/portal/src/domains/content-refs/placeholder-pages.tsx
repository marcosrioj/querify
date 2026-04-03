import { EmptyState } from '@/shared/ui/placeholder-state';
import { PageHeader } from '@/shared/layout/page-layouts';

export function ContentRefPlaceholderPage() {
  return (
    <div className="space-y-6">
      <PageHeader
        eyebrow="Sources"
        title="Sources"
        description="This page is still being connected."
      />
      <EmptyState
        title="Source page coming soon"
        description="The real list, detail, create, and edit flow will show here."
      />
    </div>
  );
}
