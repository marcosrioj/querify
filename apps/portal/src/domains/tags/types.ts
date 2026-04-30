export type TagDto = {
  id: string;
  tenantId: string;
  name: string;
  spaceUsageCount: number;
  questionUsageCount: number;
  lastUpdatedAtUtc?: string | null;
};

export type TagCreateRequestDto = {
  name: string;
};

export type TagUpdateRequestDto = TagCreateRequestDto;
