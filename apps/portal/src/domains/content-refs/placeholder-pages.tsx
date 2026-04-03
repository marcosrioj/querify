import { EmptyState } from '@/shared/ui/placeholder-state';
import { PageHeader } from '@/shared/layout/page-layouts';

export function ContentRefPlaceholderPage() {
  return (
    <div className="space-y-6">
      <PageHeader
        eyebrow="Content Refs"
        title="Content reference management"
        description="The route boundary is in place. The next pass wires the live Portal content ref CRUD surface."
      />
      <EmptyState
        title="Content ref CRUD wiring in progress"
        description="This route will be replaced by live list, detail, create, and edit flows backed by `api/faqs/content-ref`."
      />
    </div>
  );
}
