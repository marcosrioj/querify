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
  SourceUploadStatus,
  SpaceStatus,
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
  sourceUploadStatusPresentation,
  spaceStatusPresentation,
  tenantEditionPresentation,
  tenantUserRolePresentation,
  type BadgeVariant,
  type EnumPresentation,
  visibilityPresentation,
} from '@/shared/constants/enum-ui';
import { usePortalI18n } from '@/shared/lib/use-portal-i18n';
import { Badge } from '@/shared/ui';

const unknownPresentation: EnumPresentation = {
  label: 'Unknown',
  description: 'The API returned an unsupported value.',
  badgeVariant: 'outline',
  sortGroup: Number.MAX_SAFE_INTEGER,
};

function getPresentation<T extends string | number>(
  presentations: Partial<Record<T, EnumPresentation>>,
  value: T | null | undefined,
) {
  return value === null || value === undefined
    ? unknownPresentation
    : presentations[value] ?? unknownPresentation;
}

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
  const presentation = getPresentation(tenantUserRolePresentation, role);

  return <BadgeText text={presentation.label} variant={presentation.badgeVariant} />;
}

export function TenantEditionBadge({ edition }: { edition: TenantEdition }) {
  const presentation = getPresentation(tenantEditionPresentation, edition);

  return <BadgeText text={presentation.label} variant={presentation.badgeVariant} />;
}

export function SpaceStatusBadge({ status }: { status: SpaceStatus }) {
  const presentation = getPresentation(spaceStatusPresentation, status);

  return <BadgeText text={presentation.label} variant={presentation.badgeVariant} />;
}

export function VisibilityBadge({ visibility }: { visibility: VisibilityScope }) {
  const presentation = getPresentation(visibilityPresentation, visibility);

  return <BadgeText text={presentation.label} variant={presentation.badgeVariant} />;
}

export function QuestionStatusBadge({ status }: { status: QuestionStatus }) {
  const presentation = getPresentation(questionStatusPresentation, status);

  return <BadgeText text={presentation.label} variant={presentation.badgeVariant} />;
}

export function ChannelKindBadge({ kind }: { kind: ChannelKind }) {
  const presentation = getPresentation(channelKindPresentation, kind);

  return <BadgeText text={presentation.label} variant={presentation.badgeVariant} />;
}

export function AnswerKindBadge({ kind }: { kind: AnswerKind }) {
  const presentation = getPresentation(answerKindPresentation, kind);

  return <BadgeText text={presentation.label} variant={presentation.badgeVariant} />;
}

export function AnswerStatusBadge({ status }: { status: AnswerStatus }) {
  const presentation = getPresentation(answerStatusPresentation, status);

  return <BadgeText text={presentation.label} variant={presentation.badgeVariant} />;
}

export function SourceKindBadge({ kind }: { kind: SourceKind }) {
  const presentation = getPresentation(sourceKindPresentation, kind);

  return <BadgeText text={presentation.label} variant={presentation.badgeVariant} />;
}

export function SourceRoleBadge({ role }: { role: SourceRole }) {
  const presentation = getPresentation(sourceRolePresentation, role);

  return <BadgeText text={presentation.label} variant={presentation.badgeVariant} />;
}

export function SourceUploadStatusBadge({ status }: { status: SourceUploadStatus }) {
  const presentation = getPresentation(sourceUploadStatusPresentation, status);

  return <BadgeText text={presentation.label} variant={presentation.badgeVariant} />;
}

export function ActivityKindBadge({ kind }: { kind: ActivityKind }) {
  const presentation = getPresentation(activityKindPresentation, kind);

  return <BadgeText text={presentation.label} variant={presentation.badgeVariant} />;
}

export function ActorKindBadge({ kind }: { kind: ActorKind }) {
  const presentation = getPresentation(actorKindPresentation, kind);

  return <BadgeText text={presentation.label} variant={presentation.badgeVariant} />;
}
