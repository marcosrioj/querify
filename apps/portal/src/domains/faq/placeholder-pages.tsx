import { EmptyState } from '@/shared/ui/placeholder-state';
import { PageHeader } from '@/shared/layout/page-layouts';

export function FaqPlaceholderPage() {
  return (
    <div className="space-y-6">
      <PageHeader
        title="FAQs"
        description="This page is still being connected."
      />
      <EmptyState
        title="FAQ page coming soon"
        description="The real list, detail, create, edit, and generation flow will show here."
      />
    </div>
  );
}

export function FaqDetailPlaceholderPage() {
  return <FaqPlaceholderPage />;
}
