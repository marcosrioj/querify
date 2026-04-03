import { EmptyState } from '@/shared/ui/placeholder-state';
import { PageHeader } from '@/shared/layout/page-layouts';

export function FaqPlaceholderPage() {
  return (
    <div className="space-y-6">
      <PageHeader
        eyebrow="FAQ"
        title="FAQ management"
        description="The route boundary is in place. The next pass wires the real FAQ Portal CRUD adapters, detail flow, and forms."
      />
      <EmptyState
        title="FAQ CRUD wiring in progress"
        description="This foundation route will be replaced by the real FAQ list, detail, create, edit, and generation request flows."
      />
    </div>
  );
}

export function FaqDetailPlaceholderPage() {
  return <FaqPlaceholderPage />;
}
