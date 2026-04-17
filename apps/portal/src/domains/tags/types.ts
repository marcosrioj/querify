export type TagDto = {
  id: string;
  tenantId: string;
  name: string;
};

export type TagCreateRequestDto = {
  name: string;
};

export type TagUpdateRequestDto = TagCreateRequestDto;
