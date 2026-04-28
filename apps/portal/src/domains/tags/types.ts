export type TagDto = {
  id: string;
  tenantId: string;
  name: string;
  spaceUsageCount: number;
  questionUsageCount: number;
};

export type TagCreateRequestDto = {
  name: string;
};

export type TagUpdateRequestDto = TagCreateRequestDto;
