import { ActivityKind, ActorKind } from '@/shared/constants/backend-enums';

export type ActivityDto = {
  id: string;
  tenantId: string;
  questionId: string;
  questionTitle?: string | null;
  answerId?: string | null;
  answerHeadline?: string | null;
  kind: ActivityKind;
  actorKind: ActorKind;
  actorLabel?: string | null;
  userPrint: string;
  ip: string;
  userAgent: string;
  notes?: string | null;
  metadataJson?: string | null;
  occurredAtUtc: string;
};
