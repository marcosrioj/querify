import { ActivityKind, ActorKind } from '@/shared/constants/backend-enums';

export type ActivityDto = {
  id: string;
  tenantId: string;
  questionId: string;
  answerId?: string | null;
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
