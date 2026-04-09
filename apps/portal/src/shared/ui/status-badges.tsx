import { PortalRole } from '@/platform/auth/types';
import {
  ContentRefKind,
  FaqStatus,
  TenantEdition,
  TenantUserRoleType,
  contentRefKindLabels,
  faqStatusLabels,
  tenantEditionLabels,
  tenantUserRoleTypeLabels,
} from '@/shared/constants/backend-enums';
import { usePortalI18n } from '@/shared/lib/use-portal-i18n';
import { Badge } from '@/shared/ui';

export function RoleBadge({ role }: { role: PortalRole }) {
  const { t } = usePortalI18n();

  return (
    <Badge variant={role === 'Admin' ? 'primary' : 'secondary'}>{t(role)}</Badge>
  );
}

export function TenantUserRoleBadge({ role }: { role: TenantUserRoleType }) {
  const { t } = usePortalI18n();

  return (
    <Badge variant={role === TenantUserRoleType.Owner ? 'primary' : 'secondary'}>
      {t(tenantUserRoleTypeLabels[role])}
    </Badge>
  );
}

export function TenantEditionBadge({ edition }: { edition: TenantEdition }) {
  const { t } = usePortalI18n();

  return <Badge variant="outline">{t(tenantEditionLabels[edition])}</Badge>;
}

export function FaqStatusBadge({ status }: { status: FaqStatus }) {
  const { t } = usePortalI18n();
  const variant =
    status === FaqStatus.Published
      ? 'success'
      : status === FaqStatus.Archived
        ? 'mono'
        : 'warning';

  return <Badge variant={variant}>{t(faqStatusLabels[status])}</Badge>;
}

export function ContentRefKindBadge({
  kind,
}: {
  kind: ContentRefKind;
}) {
  const { t } = usePortalI18n();

  return <Badge variant="secondary">{t(contentRefKindLabels[kind])}</Badge>;
}
