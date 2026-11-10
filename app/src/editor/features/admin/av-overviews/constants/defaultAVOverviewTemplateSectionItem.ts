import { type AVOverviewItemTypeName, type IAVOverviewTemplateSectionItemModel } from '@/shared';

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
