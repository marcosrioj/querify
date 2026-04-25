import { PortalRole } from '@/platform/auth/types';
import {
  ActivityKind,
  ActorKind,
  AnswerKind,
  AnswerStatus,
  ChannelKind,
  QuestionStatus,
  SourceKind,
  SourceRole,
  SpaceKind,
  TenantEdition,
  TenantUserRoleType,
  VisibilityScope,
  activityKindLabels,
  actorKindLabels,
  answerKindLabels,
  answerStatusLabels,
  channelKindLabels,
  questionStatusLabels,
  sourceKindLabels,
  sourceRoleLabels,
  spaceKindLabels,
  tenantEditionLabels,
  tenantUserRoleTypeLabels,
  visibilityScopeLabels,
} from '@/shared/constants/backend-enums';
import { usePortalI18n } from '@/shared/lib/use-portal-i18n';
import { Badge } from '@/shared/ui';

function BadgeText({
  text,
  variant,
}: {
  text: string;
  variant:
    | 'default'
    | 'primary'
    | 'secondary'
    | 'outline'
    | 'mono'
    | 'destructive'
    | 'warning'
    | 'success'
    | 'info';
}) {
  const { t } = usePortalI18n();

  return <Badge variant={variant}>{t(text)}</Badge>;
}

export function RoleBadge({ role }: { role: PortalRole }) {
  return <BadgeText text={role} variant={role === 'Admin' ? 'primary' : 'secondary'} />;
}

export function TenantUserRoleBadge({ role }: { role: TenantUserRoleType }) {
  return (
    <BadgeText
      text={tenantUserRoleTypeLabels[role]}
      variant={role === TenantUserRoleType.Owner ? 'primary' : 'secondary'}
    />
  );
}

export function TenantEditionBadge({ edition }: { edition: TenantEdition }) {
  return <BadgeText text={tenantEditionLabels[edition]} variant="outline" />;
}

export function SpaceKindBadge({ kind }: { kind: SpaceKind }) {
  return <BadgeText text={spaceKindLabels[kind]} variant="primary" />;
}

export function VisibilityBadge({ visibility }: { visibility: VisibilityScope }) {
  const variant =
    visibility === VisibilityScope.Internal
      ? 'mono'
      : visibility === VisibilityScope.Authenticated
        ? 'info'
        : visibility === VisibilityScope.Public
          ? 'secondary'
          : 'success';

  return <BadgeText text={visibilityScopeLabels[visibility]} variant={variant} />;
}

export function QuestionStatusBadge({ status }: { status: QuestionStatus }) {
  let variant:
    | 'default'
    | 'primary'
    | 'secondary'
    | 'outline'
    | 'mono'
    | 'destructive'
    | 'warning'
    | 'success'
    | 'info' = 'outline';

  switch (status) {
    case QuestionStatus.Draft:
      variant = 'warning';
      break;
    case QuestionStatus.PendingReview:
      variant = 'info';
      break;
    case QuestionStatus.Open:
      variant = 'secondary';
      break;
    case QuestionStatus.Answered:
      variant = 'success';
      break;
    case QuestionStatus.Validated:
      variant = 'primary';
      break;
    case QuestionStatus.Escalated:
      variant = 'destructive';
      break;
    case QuestionStatus.Duplicate:
      variant = 'mono';
      break;
    case QuestionStatus.Archived:
      variant = 'outline';
      break;
  }

  return <BadgeText text={questionStatusLabels[status]} variant={variant} />;
}

export function ChannelKindBadge({ kind }: { kind: ChannelKind }) {
  return <BadgeText text={channelKindLabels[kind]} variant="outline" />;
}

export function AnswerKindBadge({ kind }: { kind: AnswerKind }) {
  return <BadgeText text={answerKindLabels[kind]} variant="secondary" />;
}

export function AnswerStatusBadge({ status }: { status: AnswerStatus }) {
  let variant:
    | 'default'
    | 'primary'
    | 'secondary'
    | 'outline'
    | 'mono'
    | 'destructive'
    | 'warning'
    | 'success'
    | 'info' = 'outline';

  switch (status) {
    case AnswerStatus.Draft:
      variant = 'warning';
      break;
    case AnswerStatus.PendingReview:
      variant = 'info';
      break;
    case AnswerStatus.Published:
      variant = 'success';
      break;
    case AnswerStatus.Validated:
      variant = 'primary';
      break;
    case AnswerStatus.Rejected:
      variant = 'destructive';
      break;
    case AnswerStatus.Obsolete:
      variant = 'mono';
      break;
    case AnswerStatus.Archived:
      variant = 'outline';
      break;
  }

  return <BadgeText text={answerStatusLabels[status]} variant={variant} />;
}

export function SourceKindBadge({ kind }: { kind: SourceKind }) {
  return <BadgeText text={sourceKindLabels[kind]} variant="secondary" />;
}

export function SourceRoleBadge({ role }: { role: SourceRole }) {
  return <BadgeText text={sourceRoleLabels[role]} variant="outline" />;
}

export function ActivityKindBadge({ kind }: { kind: ActivityKind }) {
  return <BadgeText text={activityKindLabels[kind]} variant="outline" />;
}

export function ActorKindBadge({ kind }: { kind: ActorKind }) {
  const variant = kind === ActorKind.Moderator ? 'primary' : kind === ActorKind.System ? 'mono' : 'secondary';

  return <BadgeText text={actorKindLabels[kind]} variant={variant} />;
}
