import { AVOverviewItemTypeName, IAVOverviewSectionItemModel } from 'tno-core';

export const defaultAVOverviewSectionItem = (
  sectionId: number,
  itemType: AVOverviewItemTypeName,
  time: string,
  sortOrder: number,
): IAVOverviewSectionItemModel => ({
  id: 0,
  sectionId,
  itemType,
  time,
  summary: '',
  sortOrder,
});
