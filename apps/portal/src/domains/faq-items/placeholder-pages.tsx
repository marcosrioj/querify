import { EmptyState } from '@/shared/ui/placeholder-state';
import { PageHeader } from '@/shared/layout/page-layouts';

export function FaqItemPlaceholderPage() {
  return (
    <div className="space-y-6">
      <PageHeader
        eyebrow="Q&A items"
        title="Q&A items"
        description="This page is still being connected."
      />
      <EmptyState
        title="Q&A item page coming soon"
        description="The real list and edit flow will show here."
      />
    </div>
  );
}
