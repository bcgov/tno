import { AVOverviewItemTypeName, IAVOverviewTemplateSectionItemModel } from 'tno-core';

export const defaultAVOverviewTemplateSectionItem = (
  sectionId: number,
  itemType: AVOverviewItemTypeName,
  time: string,
  sortOrder: number,
): IAVOverviewTemplateSectionItemModel => ({
  id: 0,
  sectionId,
  itemType,
  time,
  summary: '',
  sortOrder,
});
