import { PortalRole } from '@/platform/auth/types';
import {
  ContentRefKind,
  FaqSortStrategy,
  FaqStatus,
  TenantEdition,
  contentRefKindLabels,
  faqSortStrategyLabels,
  faqStatusLabels,
  tenantEditionLabels,
} from '@/shared/constants/backend-enums';
import { Badge } from '@/shared/ui';

export function RoleBadge({ role }: { role: PortalRole }) {
  return (
    <Badge variant={role === 'Admin' ? 'primary' : 'secondary'}>{role}</Badge>
  );
}

export function TenantEditionBadge({ edition }: { edition: TenantEdition }) {
  return <Badge variant="outline">{tenantEditionLabels[edition]}</Badge>;
}

export function FaqStatusBadge({ status }: { status: FaqStatus }) {
  const variant =
    status === FaqStatus.Published
      ? 'success'
      : status === FaqStatus.Archived
        ? 'mono'
        : 'warning';

  return <Badge variant={variant}>{faqStatusLabels[status]}</Badge>;
}

export function SortStrategyBadge({
  value,
}: {
  value: FaqSortStrategy;
}) {
  return <Badge variant="outline">{faqSortStrategyLabels[value]}</Badge>;
}

export function ContentRefKindBadge({
  kind,
}: {
  kind: ContentRefKind;
}) {
  return <Badge variant="secondary">{contentRefKindLabels[kind]}</Badge>;
}
