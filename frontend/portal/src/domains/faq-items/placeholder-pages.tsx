import { EmptyState } from '@/shared/ui/placeholder-state';
import { PageHeader } from '@/shared/layout/page-layouts';

export function FaqItemPlaceholderPage() {
  return (
    <div className="space-y-6">
      <PageHeader
        eyebrow="FAQ Items"
        title="FAQ item management"
        description="The route boundary is in place. The next pass wires the real FAQ Item Portal CRUD adapters, detail flow, and form schema."
      />
      <EmptyState
        title="FAQ item CRUD wiring in progress"
        description="This route will be replaced by the live list and edit flows backed by `api/faqs/faq-item`."
      />
    </div>
  );
}
