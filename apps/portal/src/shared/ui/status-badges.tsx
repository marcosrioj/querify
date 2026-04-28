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
} from '@/shared/constants/backend-enums';
import {
  activityKindPresentation,
  actorKindPresentation,
  answerKindPresentation,
  answerStatusPresentation,
  channelKindPresentation,
  questionStatusPresentation,
  sourceKindPresentation,
  sourceRolePresentation,
  spaceKindPresentation,
  tenantEditionPresentation,
  tenantUserRolePresentation,
  type BadgeVariant,
  visibilityPresentation,
} from '@/shared/constants/enum-ui';
import { usePortalI18n } from '@/shared/lib/use-portal-i18n';
import { Badge } from '@/shared/ui';

function BadgeText({
  text,
  variant,
}: {
  text: string;
  variant: BadgeVariant;
}) {
  const { t } = usePortalI18n();

  return <Badge variant={variant} appearance="outline">{t(text)}</Badge>;
}

export function RoleBadge({ role }: { role: PortalRole }) {
  return <BadgeText text={role} variant={role === 'Admin' ? 'primary' : 'secondary'} />;
}

export function TenantUserRoleBadge({ role }: { role: TenantUserRoleType }) {
  const presentation = tenantUserRolePresentation[role];

  return <BadgeText text={presentation.label} variant={presentation.badgeVariant} />;
}

export function TenantEditionBadge({ edition }: { edition: TenantEdition }) {
  const presentation = tenantEditionPresentation[edition];

  return <BadgeText text={presentation.label} variant={presentation.badgeVariant} />;
}

export function SpaceKindBadge({ kind }: { kind: SpaceKind }) {
  const presentation = spaceKindPresentation[kind];

  return <BadgeText text={presentation.label} variant={presentation.badgeVariant} />;
}

export function VisibilityBadge({ visibility }: { visibility: VisibilityScope }) {
  const presentation = visibilityPresentation[visibility];

  return <BadgeText text={presentation.label} variant={presentation.badgeVariant} />;
}

export function QuestionStatusBadge({ status }: { status: QuestionStatus }) {
  const presentation = questionStatusPresentation[status];

  return <BadgeText text={presentation.label} variant={presentation.badgeVariant} />;
}

export function ChannelKindBadge({ kind }: { kind: ChannelKind }) {
  const presentation = channelKindPresentation[kind];

  return <BadgeText text={presentation.label} variant={presentation.badgeVariant} />;
}

export function AnswerKindBadge({ kind }: { kind: AnswerKind }) {
  const presentation = answerKindPresentation[kind];

  return <BadgeText text={presentation.label} variant={presentation.badgeVariant} />;
}

export function AnswerStatusBadge({ status }: { status: AnswerStatus }) {
  const presentation = answerStatusPresentation[status];

  return <BadgeText text={presentation.label} variant={presentation.badgeVariant} />;
}

export function SourceKindBadge({ kind }: { kind: SourceKind }) {
  const presentation = sourceKindPresentation[kind];

  return <BadgeText text={presentation.label} variant={presentation.badgeVariant} />;
}

export function SourceRoleBadge({ role }: { role: SourceRole }) {
  const presentation = sourceRolePresentation[role];

  return <BadgeText text={presentation.label} variant={presentation.badgeVariant} />;
}

export function ActivityKindBadge({ kind }: { kind: ActivityKind }) {
  const presentation = activityKindPresentation[kind];

  return <BadgeText text={presentation.label} variant={presentation.badgeVariant} />;
}

export function ActorKindBadge({ kind }: { kind: ActorKind }) {
  const presentation = actorKindPresentation[kind];

  return <BadgeText text={presentation.label} variant={presentation.badgeVariant} />;
}
